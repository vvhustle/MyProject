using System.Collections;
using Yurowm.ContentManager;

namespace YMatchThree.Core {
    public class MixableChipMix : ChipMix {
        IMixableChip mixable;
        Chip center;
        Chip second;

        protected override void Prepare(Chip chipA, Chip chipB) {
            IMixableChip mixableA = chipA as IMixableChip;
            IMixableChip mixableB = chipB as IMixableChip;

            if (mixableA != null && mixableB != null) {
                center = mixableA.MixPriority < mixableB.MixPriority ? chipA : chipB;
                second = center == chipA ? chipB : chipA;
            } else if (mixableA != null) {
                center = chipA;
                second = chipB;
            } else if (mixableB != null) {
                center = chipB;
                second = chipA;
            } else {
                UnityEngine.Debug.LogError("At least one of the chips must be mixable with the second one");
                return;
            }
            
            mixable = center as IMixableChip;
            
            var dA = chipA as IDestroyable;
            var dB = chipB as IDestroyable;
            
            dA?.MarkAsDestroyed();
            dB?.MarkAsDestroyed();

            if (context.SetupItem(out Score score)) {
                var points = 0;
                if (dA != null) points += dA.scoreReward;
                if (dB != null) points += dB.scoreReward;
                score.AddScore(points);
            }

            ApplyColor(center, second);
            
            mixable.PrepareMixWith(second);
            second.HideAndKill();
        }

        public override IEnumerator Logic() {
            if (mixable != null)
                yield return mixable.MixLogic();
            if (center.IsAlive() && !center.destroying)
                center.HideAndKill();
        }
    }
    
    public interface IMixableChip {
        int MixPriority {get; set;}

        void PrepareMixWith(Chip secondChip);

        IEnumerator MixLogic();
    }
}