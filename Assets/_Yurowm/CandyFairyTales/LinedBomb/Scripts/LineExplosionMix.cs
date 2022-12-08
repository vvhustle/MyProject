using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Yurowm;
using Yurowm.ContentManager;
using Yurowm.Coroutines;
using Yurowm.Extensions;
using Yurowm.Serialization;
using Yurowm.Utilities;
using static Yurowm.Effects.LinedExplosionLogicProvider;
using LineInfo = Yurowm.Effects.LinedExplosionLogicProvider.LineInfo;

namespace YMatchThree.Core {
    public class LineExplosionMix : ChipMix {

        public LinedBombHit hit = new LinedBombHit();
            
        protected override void Prepare(Chip centerChip, Chip secondChip) {
            if (context.SetupItem(out Score score)) {
                var points = 0;
                if (centerChip is IDestroyable d1) points += d1.scoreReward;
                if (secondChip is IDestroyable d2) points += d2.scoreReward;
                score.AddScore(points);
            }
            
            ApplyColor(centerChip, secondChip);
            
            centerChip.HideAndKill();
            secondChip.HideAndKill();
            
            hit.Initialize(slot.GetTempModule(), context);
            hit.Prepare(false);
        }

        public override IEnumerator Logic() {
            var destroyingClip = lcAnimator.PlayClip("Destroying");
            
            yield return hit.Logic();
            
            yield return lcAnimator.StopAndWait(destroyingClip);
        }

        public override IEnumerable GetEffectCallbacks() {
            yield return base.GetEffectCallbacks();
            yield return hit.GetDestroyingEffectCallbacks();
        }

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
}