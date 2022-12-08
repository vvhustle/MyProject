using System.Collections;
using System.Collections.Generic;
using Yurowm.ContentManager;
using Yurowm.Coroutines;
using Yurowm.Effects;
using Yurowm.Serialization;
using Yurowm.Utilities;

namespace YMatchThree.Core {
    public class ExplosionMix : ChipMix {
        
        public BombHit hit = new BombHit();

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
        }

        public override IEnumerator Logic() {
            hit.Initialize(slot.GetTempModule(), context);
            
            hit.Prepare();
            
            yield return hit.Logic();
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