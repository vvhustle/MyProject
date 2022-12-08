using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using Yurowm;
using Yurowm.ContentManager;
using Yurowm.Effects;
using Yurowm.Extensions;
using Yurowm.Serialization;
using Yurowm.UI;
using Space = Yurowm.Spaces.Space;

namespace YMatchThree.Core {
    public abstract class CollectionGoal : LevelGoal, ISlotRatingProvider {
        
        public int count = 10;
        public bool all = false;
        
        public int collected = 0;
        
        public string collectingEffect;
        
        public override void OnAddToSpace(Space space) {
            base.OnAddToSpace(space);
            
            events.onStartDestroying += OnStartDestroying;
            if (all)
                events.onStartTask += OnStartTask;
        }

        void OnStartDestroying(SlotContent content, HitContext hitContext) {
            if (!ContentFilter(content)) return;

            void Collect() {
                if (!all) this.Collect();
                UIRefresh.Invoke();
            }
            
            #region Collecting Effect

            if (!collectingEffect.IsNullOrEmpty()) {
                var destinationObject = counterBodies
                    .FirstOrDefault(b => b.isActiveAndEnabled)?
                    .iconRenderers
                    .FirstOrDefault()?
                    .rectTransform;
            
                if (destinationObject) {
                    var contentTransform = content.body?.transform;
                    
                    if (contentTransform) {
                        var callback = new TransitionEffectLogicProvider.Callback();
                        IDisposable cameraLock = null;
                        
                        callback.destinationPoint = destinationObject.position;
                        callback.setupBody = b => {
                            var iconSR = b.transform
                                .GetChildByName("Icon")?.GetComponent<SpriteRenderer>();
                            
                            var sourceSR = contentTransform
                                .GetChildByName("Icon")?.GetComponent<SpriteRenderer>();

                            if (!iconSR || !sourceSR) return;

                            iconSR.sprite = sourceSR.sprite;
                            iconSR.color = sourceSR.color;
                            
                            cameraLock = Page.NewActiveAnimation();
                        };
                        
                        callback.onComplete += () => {
                            cameraLock?.Dispose();
                            Collect();
                        };
                        
                        var effect = Effect.Emit(space, collectingEffect, contentTransform.position, callback);
                            
                        
                        effect.size = contentTransform.lossyScale.x;
                        
                        if (effect)
                            return;
                    }
                }
            }

            #endregion

            Collect();
        }

        void OnStartTask(LevelGameplay.Task task) {
            if (all && task is WaitTask)
                UIRefresh.Invoke();
        }
        
        protected abstract bool ContentFilter(SlotContent content);

        void Collect() {
            if (collected < count)
                collected++;
        }

        public override bool IsFailed() {
            return false;
        }

        public override string GetCounterValue() {
            if (all)
                return context.Count<SlotContent>(ValidateExistedContent).ToString();
            else
                return (count - collected).ToString();
        }

        bool ValidateExistedContent(SlotContent content) {
            return content.slotModule.HasSlot() 
                   && (!(content is IDestroyable destroyable) || !destroyable.destroying)
                   && ContentFilter(content);
        }
        
        bool isComplete = false;
        
        public override bool IsComplete() {
            if (isComplete) return true;
            
            if (all)
                isComplete = !context.Contains<SlotContent>(ValidateExistedContent);
            else
                isComplete = collected >= count;
            
            if (isComplete) {
                events.onStartDestroying -= OnStartDestroying;
                events.onStartTask -= OnStartTask;
            }
            
            return isComplete;
        }

        public override IEnumerator GetVariblesTypes() {
            yield return base.GetVariblesTypes();
            yield return typeof(CollectionSizeVariable);
        }

        public override void SetupVariable(ISerializable variable) {
            base.SetupVariable(variable);
            switch (variable) {
                case CollectionSizeVariable size: {
                    count = size.count; 
                    all = size.all; 
                    return;
                }
            }
        }

        public int RateSlot(Slot slot) {
            if (isComplete) return 0;
            
            var content = slot.GetCurrentContent();
            
            if (!content) return 0;
            
            if (content is IDestroyable && ContentFilter(content))
                return 10;

            return 0;
        }

        public override void Serialize(Writer writer) {
            base.Serialize(writer);
            writer.Write("collectingEffect", collectingEffect);
        }

        public override void Deserialize(Reader reader) {
            base.Deserialize(reader);
            reader.Read("collectingEffect", ref collectingEffect);
        }
    }
    
    public class CollectionSizeVariable : ContentInfoVariable {
        public int count = 10;
        public bool all = false;
        
        public override void Serialize(Writer writer) {
            writer.Write("colall", all);
            if (!all)
                writer.Write("colcount", count);
        }

        public override void Deserialize(Reader reader) {
            reader.Read("colall", ref all);
            if (!all)
                reader.Read("colcount", ref count);
        }
    }
}