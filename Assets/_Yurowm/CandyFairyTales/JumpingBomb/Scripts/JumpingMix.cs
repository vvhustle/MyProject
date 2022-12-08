using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using Yurowm.ContentManager;
using Yurowm.Coroutines;
using Yurowm.Effects;
using Yurowm.Extensions;
using Yurowm.Serialization;
using Yurowm.Utilities;

namespace YMatchThree.Core {
    public class JumpingMix : ChipMix {
        ItemColorInfo colorInfo;
        
        public int count = 3;
        public string nodeEffectName = null;
        
        public ExplosionParameters startExplosion = new ExplosionParameters();
        public ExplosionParameters endExplosion = new ExplosionParameters();
        
        JumpingBomb.SlotRating rating;
        
        protected override void Prepare(Chip centerChip, Chip secondChip) {
            if (secondChip is IColored c1)
                colorInfo = c1.colorInfo;
            else if (centerChip is IColored c2)
                colorInfo = c2.colorInfo;
            else
                colorInfo = ItemColorInfo.ByID(context.GetArgument<LevelColorSettings>().GetRandomColorID());
            
            centerChip.HideAndKill();
            secondChip.HideAndKill();
            
            rating = JumpingBomb.SlotRating.CreateOrFind(context);
            rating.Refresh();
        }
        
        public override IEnumerator Logic() {
            yield return Enumerator
                .For(0, count, 1)
                .Select(i => Jump())
                .Parallel();
        }

        
        IEnumerator Jump() {
            var effectCallback = new FlyEffectLogicProvider.Callback();
            var repaintCallback = new RepaintEffectLogicProvider.Callback();
            repaintCallback.colorInfo = colorInfo;
            
            rating = JumpingBomb.SlotRating.CreateOrFind(context);
            
            SlotContent content = null;
            Slot target = null;

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
            
            effectCallback.accuracy = Slot.Offset;
            effectCallback.pullTarget = () => {
                if (!content || content.destroying) 
                    UpdateTarget();
                return content?.position ?? Vector2.zero;
            };
            
            effectCallback.onComplete = () => {
                rating.SetBusy(target, false);
                
                field.Explode(position, endExplosion);
            
                field.Highlight(new [] { target }, target);
                
                target.HitAndScore(new HitContext(context, target, HitReason.BombExplosion));
            };
            
            var effect = Effect.Emit(field, nodeEffectName, position, 
                effectCallback, repaintCallback);
            
            if (!effect) {
                effectCallback.onComplete.Invoke();   
                yield break;
            }
            
            field.Explode(position, startExplosion);

            while (effect.IsAlive())
                yield return null;
        }

        public override void Serialize(Writer writer) {
            base.Serialize(writer);
            writer.Write("nodeEffectName", nodeEffectName);
            writer.Write("count", count);
            writer.Write("startExplosion", startExplosion);
            writer.Write("endExplosion", endExplosion);
        }

        public override void Deserialize(Reader reader) {
            base.Deserialize(reader);
            reader.Read("nodeEffectName", ref nodeEffectName);
            reader.Read("count", ref count);
            reader.Read("startExplosion", ref startExplosion);
            reader.Read("endExplosion", ref endExplosion);
        }
    }
}