using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YMatchThree.Core {
    public class SimpleModifier : SlotModifier, ILayered {
        public int scoreReward { get; set; }
        public int layersCount { get; set; }
        public int layer { get; set; }
        public int layerScoreReward { get; set; }
        public string layerDownEffect { get; set; }
            
        public IEnumerator Destroying() {
            yield return lcAnimator.PlayClipAndWait("Destroying");
        }

        public void OnChangeLayer(int layer) {
            lcAnimator.PlayClip("LayerDown");
        }
    }
}