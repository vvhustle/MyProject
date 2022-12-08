using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Yurowm;
using Yurowm.ContentManager;
using Yurowm.Coroutines;
using Yurowm.Effects;
using Yurowm.Extensions;
using Yurowm.Serialization;
using Yurowm.Utilities;
using Space = Yurowm.Spaces.Space;

namespace YMatchThree.Core {
    public class JumpingBomb : BombChipBase, IMixableChip {

        public ExplosionParameters startExplosion = new ExplosionParameters();
        public ExplosionParameters endExplosion = new ExplosionParameters();
        
        public string jumpingEffectName;
        
        SlotRating rating;
        
        Slot target;
        
        public override IEnumerator Exploding() {
            if (simulation.AllowToWait())
                yield return new Wait(.1f);
            
            yield return Jump();
            
            field.Explode(position, endExplosion);
            
            if (target) {
                field.Highlight(new [] { target }, target);
                    
                target.HitAndScore(new HitContext(context, target, HitReason.BombExplosion));
            }
        }
        
        IEnumerator Jump() {
            if (!simulation.AllowAnimations())
                yield break;
            
            rating = SlotRating.CreateOrFind(context);
            
            SlotContent content = null;
            
            void UpdateTarget() {
                rating.Refresh();
                if (target != null) {
                    rating.SetBusy(target, false);
                }
                target = rating.GetTopRated().GetRandom(random);
                if (target != null) {
                    rating.SetBusy(target, true);
                    content = target.GetCurrentContent();
                }
            }

            UpdateTarget();
            
            BreakParent();
            
            var effectCallback = new FlyEffectLogicProvider.Callback();
            var repaintCallback = new RepaintEffectLogicProvider.Callback();

            repaintCallback.colorInfo = colorInfo;

            effectCallback.accuracy = Slot.Offset;
            effectCallback.pullTarget = () => {
                if (!content || content.destroying) 
                    UpdateTarget();
                return content?.position ?? Vector2.zero;
            };

            effectCallback.onComplete = () => rating.SetBusy(target, false);

            var effect = Effect.Emit(field, jumpingEffectName, position,
                effectCallback, repaintCallback);

            if (!effect) {
                effectCallback.onComplete.Invoke();   
                yield break;
            }

            field.Explode(position, startExplosion);

            body.Kill();

            while (effect.IsAlive())
                yield return null;
        }

        #region ISerializable
        
        public override void Serialize(Writer writer) {
            base.Serialize(writer);
            writer.Write("jumpingEffectName", jumpingEffectName);
            writer.Write("startExplosion", startExplosion);
            writer.Write("endExplosion", endExplosion);
            writer.Write("MixPriority", MixPriority);
        }

        public override void Deserialize(Reader reader) {
            base.Deserialize(reader);
            reader.Read("jumpingEffectName", ref jumpingEffectName);
            reader.Read("startExplosion", ref startExplosion);
            reader.Read("endExplosion", ref endExplosion);
            MixPriority = reader.Read<int>("MixPriority");
        }

        #endregion

        #region IMixableChip
        
        public int MixPriority { get; set; }
        
        BombChipBase mixedChip;
        
        public void PrepareMixWith(Chip secondChip) {
            if (this is IColored c1)
                colorInfo = c1.colorInfo;
            else if (secondChip is IColored c2)
                colorInfo = c2.colorInfo;
            else
                colorInfo = ItemColorInfo.ByID(context.GetArgument<LevelColorSettings>().GetRandomColorID());
            
            mixedChip = (secondChip as BombChipBase)?.Clone();
            secondChip.HideAndKill();
            
            if (!mixedChip)
                return;
            
            mixedChip.SetupVariable(new ColoredVariable {
                info = colorInfo
            });
        }

        public IEnumerator MixLogic() {
            var field = this.field;
            
            HideAndKill();
            
            yield return Jump();
            
            if (!target) yield break;

            target
                .GetContent<Chip>()?
                .Hit(new HitContext(context, target, HitReason.BombExplosion));

            mixedChip.emitType = EmitType.Matching;
            mixedChip.bodyName = null;
            
            field.AddContent(mixedChip);

            mixedChip.slotModule.SetCenterSlot(target);
            
            mixedChip.localPosition = default;
            mixedChip.destroyingDelay = 0;
            
            mixedChip.HitAndScore(new HitContext(context, target, HitReason.BombExplosion));
            
            while (mixedChip.IsAlive())
                yield return null;
        }

        #endregion
        
        public class SlotRating : GameEntity, ISlotRatingProvider {
            
            Dictionary<Slot, int> rating = new Dictionary<Slot, int>();
            List<Slot> busySlots = new List<Slot>();
            
            LevelEvents events;
            
            int lastMatchDate = -1;
            
            public static SlotRating CreateOrFind(LiveContext context) {
                var result = context.Get<SlotRating>();
                
                if (!result) {
                    result = new SlotRating();
                    context.Add(result);
                }
                
                return result;
            }

            public override void OnAddToSpace(Space space) {
                base.OnAddToSpace(space);
                events = context.GetArgument<LevelEvents>();
                
                events.onStartDestroying += OnStartDestroying;
            }

            public override void OnRemoveFromSpace(Space space) {
                base.OnRemoveFromSpace(space);
                
                if (events != null)
                    events.onStartDestroying -= OnStartDestroying;
            }

            void OnStartDestroying(SlotContent content, HitContext hitContext) {
                content.slotModule.Slots().ForEach(s => rating.Remove(s));
            }

            public void SetBusy(Slot slot, bool isBusy) {
                if (isBusy == IsBusy(slot))
                    return;
                
                if (isBusy)
                    busySlots.Add(slot);
                else
                    busySlots.Remove(slot);
            }
            
            public bool IsBusy(Slot slot) {
                return busySlots.Contains(slot);
            }
            
            public void Refresh() {
                var gameplay = context.Get<LevelGameplay>();
                
                if (!gameplay) return;
                
                lastMatchDate = gameplay.matchDate;
                
                rating.Clear();
                
                var ratingProviders = context
                    .GetAll<ISlotRatingProvider>()
                    .ToArray();
                
                var slots = context.Get<Slots>();
                
                foreach (var slot in slots.all.Values) {
                    if (slots.hidden.ContainsValue(slot)) 
                        continue;

                    rating.Add(slot, ratingProviders.Sum(rp => rp.RateSlot(slot)));
                }
            }
            
            public bool TryToRate(Slot slot, out int rating) {
                return this.rating.TryGetValue(slot, out rating);
            }

            public IEnumerable<Slot> GetTopRated() {
                var topRating = rating
                    .Where(p => !IsBusy(p.Key))
                    .Max(p => p.Value);

                foreach (var pair in rating) {
                    if (pair.Value == topRating && !IsBusy(pair.Key))
                        yield return pair.Key;
                }
            }

            public int RateSlot(Slot slot) {
                var content = slot.GetCurrentContent();

                if (!content || content is JumpingBomb || content.birthDate >= lastMatchDate)
                    return -9999;
                
                if (content is BombChipBase)
                    return 1;
                
                return 0;
            }
        }
    }
    
    public interface ISlotRatingProvider : ILiveContexted {
        int RateSlot(Slot slot);
    }
}