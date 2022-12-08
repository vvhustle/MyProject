using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using YMatchThree.Meta;
using Yurowm;
using Yurowm.Controls;
using Yurowm.Core;
using Yurowm.Coroutines;
using Yurowm.Extensions;
using Yurowm.Localizations;
using Yurowm.Serialization;
using Yurowm.Spaces;
using Yurowm.Store;
using Yurowm.UI;

namespace YMatchThree.Core {
    public abstract class FieldBooster : Booster {

        enum State {
            Activated = 1 << 0,            
            Deactivating = 1 << 1,            
            Working = Activated | Deactivating
        }
        
        State state;
        
        public LocalizedText descriptionText = new LocalizedText();
        
        public bool IsActivated => state.HasFlag(State.Activated) && !state.HasFlag(State.Deactivating);
        
        public bool IsWorking => state.OverlapFlag(State.Working);

        public override void OnClick() {
            if (IsActivated) {
                Deactivate();
            } else {
                if (PlayerData.inventory.GetItemCount(ID) > 0)
                    Activate();
                else
                    ShowInStore().Run();
            } 
        }

        void Activate() {
            if (IsWorking)
                return;
            InternalLogic().Run(field.coroutine);
        }
        
        public void Deactivate() {
            if (IsActivated) 
                state = state.With(State.Deactivating);
        }

        public override void SetupBody(VirtualizedScrollItemBody body) {
            base.SetupBody(body);
            if (body.SetupChildComponent(out ContentAnimator animator)) {
                if (IsActivated) 
                    animator.Play("Selected", WrapMode.Loop);
                else
                    animator.Rewind("Selected");
            }
        }

        bool busy = false;
        
        IEnumerator InternalLogic() {
            if (busy) yield break;
            
            busy = true;
            
            var activatedBoosters = context
                .GetAll<FieldBooster>()
                .Where(b => b != this && b.busy)
                .ToList();
            
            activatedBoosters.ForEach(b => b.Deactivate());
            
            while (!activatedBoosters.IsEmpty()) {
                activatedBoosters.RemoveAll(b => !b.busy);
                yield return null;
            }

            state = state.With(State.Activated);
            UIRefresh.Invoke();
            BoosterUI(bui => bui.Setup(this));
            yield return Logic();
            state = state.Without(State.Activated);
            
            state = state.With(State.Deactivating);
            BoosterUI(bui => bui.Setup(null));
            yield return Deactivation();
            state = state.Without(State.Deactivating);
            UIRefresh.Invoke();
            
            yield return Page.WaitAnimation();
            
            busy = false;
        }
        
        void BoosterUI(Action<FieldBoosterUI> bui) {
            Yurowm.Behaviour
                .GetAll<FieldBoosterUI>()
                .ForEach(bui);
        }

        protected abstract IEnumerator Logic();
        protected abstract IEnumerator Deactivation();
        
        public override IEnumerable GetLocalizationKeys() {
            yield return base.GetLocalizationKeys(); 
            yield return descriptionText; 
        }
        
        #region Lists
        
        const string boosterListID = "FieldBoosterList";
        
        public static void FillBoosterLists(Field field) {
            
            var boosterSet = field.fieldContext.Get<FieldBoosterSetBase>();
            
            if (boosterSet == null)
                return;
            
            var boosters = storage
                .Items<FieldBooster>()
                .Where(boosterSet.Filter)
                .Select(b => b.Clone())
                .Initialize(field.AddContent)
                .ToArray();
            
            Yurowm.Behaviour
                .GetAllByID<VirtualizedScroll>(boosterListID)
                .ToArray()
                .ForEach(l => {
                    l.SetList(boosters);
                    
                    if (l.isActiveAndEnabled && l.SetupComponent(out ContentAnimator animator)) {
                        animator
                            .PlayAndWait("Show")
                            .Run();
                    } 
                });
        }

        public static void ClearBoosterLists() {
            Yurowm.Behaviour
                .GetAllByID<VirtualizedScroll>(boosterListID)
                .ForEach(l => {
                    if (l.isActiveAndEnabled && l.SetupComponent(out ContentAnimator animator)) {
                        animator
                            .PlayAndWait("Hide")
                            .ContinueWith(() => l.SetList<FieldBooster>(null))
                            .Run();
                    } else {
                        l.SetList<FieldBooster>(null);
                    }
                });
        }
        
        #endregion

        #region ISerializable

        public override void Serialize(Writer writer) {
            base.Serialize(writer);
            writer.Write("descriptionText", descriptionText);
        }

        public override void Deserialize(Reader reader) {
            base.Deserialize(reader);
            reader.Read("descriptionText", ref descriptionText);
        }

        #endregion
    }
    
    public abstract class FieldBoosterSlotBased : FieldBooster {
        
        TouchControl control;
        
        Slot targetSlot;
        
        protected abstract bool FilterSlot(Slot slot);
        protected abstract IEnumerator SlotLogic(Slot slot);
        
        protected abstract bool IsAutoredeem {get;}

        void OnClick(TouchStory touch) {
            var pickedSlot = gameplay.space.clickables
                .Cast<Slot>(touch.currentWorldPosition, Slot.Offset);
            
            if (!pickedSlot) return;
            
            if (FilterSlot(pickedSlot))
                targetSlot = pickedSlot;
        }
        
        protected override IEnumerator Logic() {
            field.gameplay.ClearControl();
            
            var frc = space.context.Get<FieldRectController>();
            if (frc) {
                frc.SetMessageState(this, true);
                while (frc.IsAnimating())
                    yield return null;
            }
            
            space.context.Catch<CameraOperator>(o => {
                control = o.controls;
                control.onTouchClick += OnClick;
            });

            while (IsActivated && !targetSlot)
                yield return null;
            
            if (targetSlot && gameplay.GetCurrentTask() is WaitTask) {
                if (CanBeRedeemed()) {
                    if (IsAutoredeem)
                        Redeem();
                    
                    gameplay.HideHint();
                    
                    yield return SlotLogic(targetSlot);
                }
            }
        }

        protected override IEnumerator Deactivation() {
            if (control != null) 
                control.onTouchClick -= OnClick;
            
            targetSlot = null;
            
            var frc = space.context.Get<FieldRectController>();
            if (frc) {
                frc.SetMessageState(this, false);
                while (frc.IsAnimating())
                    yield return null;
            }
            
            field.gameplay.SetupControl();
            
            yield break;
        }
    }
}
