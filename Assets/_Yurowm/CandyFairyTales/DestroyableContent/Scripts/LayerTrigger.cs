using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Yurowm.Extensions;

namespace YMatchThree.Core {
    public abstract class LayerTrigger : MonoBehaviour {
        public int layer = 1;
        
        public void SetLayer(int layer) {
            if (this.layer < layer)
                OnLayerBelow();
            else if (this.layer > layer)
                OnLayerAbove();
            else
                OnLayer();
        }
        
        public abstract void OnLayerBelow();
        public abstract void OnLayer();
        public abstract void OnLayerAbove();
        
        public static void Trigger(GameObject parent, int layer) {
            parent
                .GetComponentsInChildren<LayerTrigger>(true)
                .ForEach(t => t.SetLayer(layer));
        }
    }
}