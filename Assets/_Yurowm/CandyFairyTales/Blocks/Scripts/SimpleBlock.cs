using System.Collections;

namespace YMatchThree.Core {
    public class SimpleBlock : Block, ILayered, INearHitDestroyable, ILineBreaker {
        public int scoreReward { get; set; } = 1;
        public int layersCount { get; set; } = 1;
        public int layer { get; set; } = 1;
        public int layerScoreReward { get; set; } = 1;
        public string layerDownEffect { get; set; }

        public IEnumerator Destroying() {
            OnChangeLayer(0);
            BreakParent(); 
            yield return lcAnimator.PlayClipAndWait("Destroying");
        }
        
        public void OnChangeLayer(int layer) {
            lcAnimator.PlayClip("LayerDown");
        }
    }
}