using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Yurowm;
using Yurowm.ContentManager;
using Yurowm.Controls;
using Yurowm.Extensions;
using Yurowm.Serialization;
using Yurowm.Shapes;
using Yurowm.Utilities;
using Object = UnityEngine.Object;
using Space = Yurowm.Spaces.Space;

namespace YMatchThree.Core {
    public class MatchChainGameplay : LevelGameplay {
        
        List<Slot> selectedSlots => slots.selection;
        
        public ExplosionParameters selectBouncing = new ExplosionParameters();
        public ExplosionParameters unselectBouncing = new ExplosionParameters();
        
        ItemColorInfo stackColor = ItemColorInfo.None;
        bool stackFilling = false;
        
        #region Events

        LevelEvents events;
        
        public Action onStackCountChanged = delegate { };
        public Action onStartFillingStack = delegate { };
        public Action<Slot[]> onReleaseStack = delegate { };

        #endregion

        public override void OnAddToSpace(Space space) {
            base.OnAddToSpace(space);
            
            line = AssetManager.Create<YLine2D>(lineName);
            line?.transform.SetParent(space.root);
            
            events = context.GetArgument<LevelEvents>();
            
            events.onAllGoalsAreReached += OnAllTargetsIsReached;
        }

        public override void OnRemoveFromSpace(Space space) {
            base.OnRemoveFromSpace(space);
            
            if (line)
                Object.Destroy(line.gameObject);
        }

        void OnStackSizeChanged() {
            
            selectedSlots
                .Select(s => s.GetCurrentContent())
                .CastIfPossible<IContentWithOverlay>()
                .ForEach(co => co.SetOverlay(null));
            
            if (GetSelectedMixRecipe() == null) {
                var combo = combinations
                    .FirstOrDefault(c => c.IsSuitable(selectedSlots));
                
                var slotForBomb = GetSlotForBomb();
                
                if (combo != null && slotForBomb?.GetCurrentContent() is IContentWithOverlay co)
                    co.SetOverlay(AssetManager.GetAsset<Sprite>(GetItem<BombChipBase>(combo.bomb)?.overlayID));
            }
            
            onStackCountChanged?.Invoke();
        }
        
        void OnAllTargetsIsReached() {
            
        }

        public override ColorBake GetColorBake() {
            return new LazyColorBake();
        }

        static readonly Side[] sidesToCheck = {
            Side.Right,
            Side.Bottom
        };
        
        public override bool IsThereAnyPotentialTurns() {
            foreach (Slot slot in slots.all.Values)
                if (slot.GetCurrentContent() is IColored coloredA)
                    foreach (var side in sidesToCheck) {
                        var next = slot[side];
                        
                        if (!next)
                            continue;

                        if (next.GetCurrentContent() is IColored coloredB)
                            if (coloredA.colorInfo.IsMatchWith(coloredB.colorInfo))
                                return true;
                    }
            
            return false;
        }

        public override bool IsThereAnySolutions() {
            return false;
        }

        #region Controls
        
        protected override void SetupControl(TouchControl control) {
            control.onTouchBegin += OnTouchBegin;
            control.onTouch += OnTouch;
            control.onTouchEnd += OnTouchEnd;
        }
        
        protected override void ClearControl(TouchControl control) {
            control.onTouchBegin -= OnTouchBegin;
            control.onTouch -= OnTouch;
            control.onTouchEnd -= OnTouchEnd;
        }
        
        Slot GetSlot(Vector2 position) {
            return space.clickables.Cast<Slot>(position, Slot.Offset);
        }
            
        void OnTouchBegin(TouchStory touch) {
            if (touch.IsOverUI) 
                return;
                
            if (!IsPlaying()) 
                return;
                
            if (!IsWaitingForPlayerTurn() || IsLevelComplete()) 
                return;
                
            if (!GetSlot(touch.currentWorldPosition)) 
                return;
                
            slots.ClearSelection();

            stackFilling = true;
            stackColor = ItemColorInfo.Unknown;
            line?.Clear();

            onStartFillingStack.Invoke();
        }
            
        void OnTouch(TouchStory touch) {
            if (!stackFilling) return;
                
            if (touch.IsOverUI) return;
                
            var point = touch.currentWorldPosition;
            
            if (!selectedSlots.IsEmpty()) {
                var lastSlot = selectedSlots.Last();
                var lastSlotPosition = space.clickables.GetWorldPoint(lastSlot.position);
                var delta = point - lastSlotPosition;
                
                if (delta.MagnitudeIsGreaterThan(Slot.Offset))
                    point = lastSlotPosition + delta.normalized * Slot.Offset;
            }
                
            var slot = GetSlot(point);

            if (slot) SelectSlot(slot);
        }
            
        void OnTouchEnd(TouchStory touch) {
            if (!stackFilling) return;
                
            onReleaseStack.Invoke(selectedSlots.ToArray());
            gameplay.NextTask<MatchingTask>();
        }
        
        #endregion

        void SelectSlot(Slot slot) {
            if (!stackFilling) return;
            
            var color = slot.GetCurrentColor();

            if (!color.IsMatchableColor())
                return;

            #region Select First Slot
            
            if (selectedSlots.IsEmpty()) {
                slots.Select(slot);
                field.Explode(slot.position, selectBouncing);
                stackColor = color;
                OnStackSizeChanged();
                UpdateStackLine();
                return;
            }

            #endregion

            #region Select Next Slot
            
            if (!selectedSlots.Contains(slot)) {
                if (CanBeSelected(slot)) {
                    var lastSlot = selectedSlots.Last();
                    foreach (var side in Sides.straight)
                        if (slot[side] && slot[side] == lastSlot) {
                            slots.Select(slot);
                            field.Explode(slot.position, selectBouncing);
                            if (!stackColor.IsKnown())
                                stackColor = color;
                            
                            OnStackSizeChanged();
                            break;
                        }
                }
            }

            #endregion

            #region Unselect Previous Slot
            
            if (selectedSlots.Count >= 2 && selectedSlots[selectedSlots.Count - 2] == slot) {
                var slotToUnselect = selectedSlots.Last();
                if (slotToUnselect.GetCurrentContent() is IContentWithOverlay co)
                    co.SetOverlay(null);
                slots.Unselect(slotToUnselect);
                field.Explode(slotToUnselect.position, unselectBouncing);

                var s = selectedSlots
                    .Find(x => x.GetCurrentColor().type != ItemColor.Universal);

                if (s)
                    stackColor = s.GetCurrentColor();
                else
                    stackColor = ItemColorInfo.Universal;

                OnStackSizeChanged();
            }

            #endregion

            line?.gameObject.Repaint(colorPalette, stackColor);
            UpdateStackLine();
        }
        
        bool CanBeSelected(Slot slot) {
            if (!stackFilling) return false;
            
            if (selectedSlots.Count == 1) {
                var mixRecipe = GetMixRecipe(selectedSlots[0], slot);
                if (mixRecipe != null)
                    return true;
            }
            
            var colorInfo = slot.GetCurrentColor();
            
            return selectedSlots.All(s => s.GetCurrentColor().IsMatchWith(colorInfo));
        }
        
        public override IEnumerator Matching() {
            stackFilling = false;
            
            if (selectedSlots.Count < 2) {
                slots.ClearSelection();
                UpdateStackLine();
                yield break;
            }

            if (!AllowToMove())
                yield break;

            OnNextTurn();

            if (GetSelectedMixRecipe() != null)
                yield return Mixing();
            else
                yield return LineMatching();

            slots.ClearSelection();
            UpdateStackLine();
            
            stackColor = ItemColorInfo.None;
        }
        
        IEnumerator LineMatching() {
            var pressedSlots = slots.selection.ToArray(); 
            var slotForBomb = GetSlotForBomb();
            
            var combo = combinations.FirstOrDefault(c => c.IsSuitable(pressedSlots));
            
            var hitContext = new HitContext(context, selectedSlots.ToArray(), HitReason.Matching);
            for (int i = selectedSlots.Count - 1; i >= 0; i--) {
                var s = selectedSlots[i];
                s.HitAndScore(hitContext);
                slots.Unselect(s);
                field.Explode(s.position, unselectBouncing);
                UpdateStackLine();
                
                yield return time.Wait(0.05f);
            }
            
            matchDate++;
            
            if (slotForBomb && combo != null) {
                var bombInfo = storage.GetItemByID<Chip>(combo.bomb);
                
                if (bombInfo) {
                    var bomb = bombInfo.Clone();
                            
                    if (stackColor.type == ItemColor.Known)
                        bomb.SetupVariable(new ColoredVariable {
                            info = stackColor
                        });

                    bomb.emitType = SlotContent.EmitType.Matching;
                    bomb.birthDate = matchDate;
                    field.AddContent(bomb);
                    slotForBomb.AddContent(bomb);
                    
                    bomb.localPosition = Vector2.zero;
                }
            }
        }
        
        Slot GetSlotForBomb() {
            return selectedSlots.FirstOrDefault(s => s.GetCurrentContent() is Chip c && c.IsDefault);
        }

        #region Line

        public string lineName;
        
        YLine2D line;

        void UpdateStackLine() {
            if (!line) return;
            line.Clear();
            foreach (var slot in selectedSlots)
                line.AddPoint(space.clickables.GetWorldPoint(slot.position));
        }
        
        #endregion
        
        #region Combinations

        public List<Combination> combinations = new List<Combination>();

        public class Combination : ISerializable {
            public string bomb;
            public int count = 4;
            public int verticalCount = 1;
            public int horizontalCount = 1;

            bool vertical => verticalCount > horizontalCount;

            public void Serialize(Writer writer) {
                writer.Write("bomb", bomb);
                writer.Write("count", count);
                writer.Write("vertical", verticalCount);
                writer.Write("horizontal", horizontalCount);
                
            }

            public void Deserialize(Reader reader) {
                reader.Read("bomb", ref bomb);
                reader.Read("count", ref count);
                reader.Read("vertical", ref verticalCount);
                reader.Read("horizontal", ref horizontalCount);
            }

            public bool IsSuitable(ICollection<Slot> slots) {
                var xmin = int.MaxValue;
                var ymin = int.MaxValue;
                var xmax = int.MinValue;
                var ymax = int.MinValue;
                
                var collectionCount = 0;
                
                slots.ForEach(s => {
                    collectionCount++;
                    xmin = xmin.ClampMax(s.coordinate.X);
                    ymin = ymin.ClampMax(s.coordinate.Y);
                    xmax = xmax.ClampMin(s.coordinate.X);
                    ymax = ymax.ClampMin(s.coordinate.Y);
                });

                if (collectionCount == 0 || collectionCount < count)
                    return false;
                
                var vert = ymax - ymin + 1;
                if (verticalCount > vert)
                    return false;

                var horiz = xmax - xmin + 1;
                if (horizontalCount > horiz)
                    return false;

                if (verticalCount != horizontalCount && vertical != vert >= horiz) 
                    return false;

                return true;
            }
        }

        #endregion

        #region Mixing

        public List<ChipMixRecipe> mixes = new List<ChipMixRecipe>();
        
        ChipMixRecipe GetMixRecipe(Slot a, Slot b) {
            if (!a || !b) return null;
            
            var aID = a.GetCurrentContent()?.ID;
            var bID = b.GetCurrentContent()?.ID;
                    
            if (aID.IsNullOrEmpty() || bID.IsNullOrEmpty())
                return null;
            
            return mixes.FirstOrDefault(m => m.Equals(aID, bID));
        }

        ChipMixRecipe GetSelectedMixRecipe() {
            if (selectedSlots.Count == 2)
                return GetMixRecipe(selectedSlots[0], selectedSlots[1]);
            return null;
        }
        
        IEnumerator Mixing() {
            var slotA = selectedSlots[0];
            var slotB = selectedSlots[1];

            var recipe = GetSelectedMixRecipe();
            
            selectedSlots.ToArray().ForEach(s => {
                slots.Unselect(s);
                field.Explode(s.position, unselectBouncing);
            });
            
            UpdateStackLine();
            
            if (recipe == null)
                yield break;
            
            var chipA = slotA.GetCurrentContent() as Chip;
            var chipB = slotB.GetCurrentContent() as Chip;
            
            var posA = chipA.position;
            var posB = chipB.position;
            
            if (!chipA || !chipB)
                yield break;
            
            const float duration = .3f;
            
            for (var t = 0f; t < duration; t += time.Delta) {
                chipA.position = Vector2.Lerp(posA, posB,
                    (t / duration).Ease(EasingFunctions.Easing.InOutQuad));
                
                yield return null;
            }

            var mix = storage.GetItemByID<ChipMix>(recipe.mix).Clone();
            mix.random = random.NewRandom();
            mix.position = chipB.position;
            mix.slot = chipB.slotModule.Slot();
            mix.SetChips(chipB, chipA);
                
            matchDate++;
            
            HideHint();
            
            field.AddContent(mix);

            while (mix.IsAlive()) 
                yield return null;
        }

        #endregion
        
        #region ISerializable

        public override void Serialize(Writer writer) {
            base.Serialize(writer);
            writer.Write("line", lineName);
            
            writer.Write("selectBouncing", selectBouncing);
            writer.Write("unselectBouncing", unselectBouncing);
            
            writer.Write("combinations", combinations.ToArray());
            writer.Write("mixes", mixes.ToArray());
        }
        
        public override void Deserialize(Reader reader) {
            base.Deserialize(reader);
            reader.Read("line", ref lineName);
            
            reader.Read("selectBouncing", ref selectBouncing);
            reader.Read("unselectBouncing", ref unselectBouncing);

            combinations = reader.ReadCollection<Combination>("combinations").ToList();
            mixes = reader.ReadCollection<ChipMixRecipe>("mixes").ToList();
        }

        #endregion
    }
}