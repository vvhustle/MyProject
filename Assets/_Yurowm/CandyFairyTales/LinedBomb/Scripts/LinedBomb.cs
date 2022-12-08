using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Yurowm;
using Yurowm.Coroutines;
using Yurowm.Effects;
using Yurowm.Extensions;
using Yurowm.Serialization;
using Yurowm.Utilities;
using LineInfo = Yurowm.Effects.LinedExplosionLogicProvider.LineInfo;

namespace YMatchThree.Core {
    public class LinedBomb : BombChipBase, IColored {

        public LinedBombHit hit = new LinedBombHit();
        
        #region Destroying

        protected override bool CanBeHit(HitContext hitContext) {
            if (hitContext.reason == HitReason.BombExplosion 
                && hitContext.driver is LinedBombHit hitter
                && hitter.side == hit.side)
                hit.side = hit.side.BitRotate(2);
            return base.CanBeHit(hitContext);
        }

        public override void OnStartDestroying() {
            base.OnStartDestroying();
            hit.Initialize(slotModule, context);
            hit.Prepare(false);
        }

        public override IEnumerator Exploding() {
            var destroyingClip = lcAnimator.PlayClip("Destroying");
            
            
            yield return hit.Logic();
            
            yield return lcAnimator.StopAndWait(destroyingClip);
        }

        public override IEnumerable GetDestroyingEffectCallbacks() {
            yield return base.GetDestroyingEffectCallbacks();
            yield return hit.GetDestroyingEffectCallbacks();
        }

        #endregion

        #region ISerializable

        public override void Serialize(Writer writer) {
            base.Serialize(writer);
            writer.Write("hit", hit);
        }

        public override void Deserialize(Reader reader) {
            base.Deserialize(reader);
            reader.Read("hit", ref hit);
        }

        #endregion
    }
    
    public interface ILineBreaker {
        bool BreakTheLine();
    }
}