using System;
using System.Collections;

namespace YMatchThree.Core {
    [SlotTag(ConsoleColor.DarkGreen, ConsoleColor.Cyan)]
    public class Jelly : SlotModifier, ILayered {
        
        #region ILayered
        
        public int scoreReward { get; set; } = 1;
        public int layersCount { get; set; } = 1;
        public int layer { get; set; } = 1;
        public int layerScoreReward { get; set; } = 1;
        public string layerDownEffect { get; set; }
        
        public IEnumerator Destroying() {
            BreakParent();
            yield return lcAnimator.PlayClipAndWait("Destroying");
        }

        public void OnChangeLayer(int layer) {
            lcAnimator.PlayClip("LayerDown");
        }

        #endregion
    }
}