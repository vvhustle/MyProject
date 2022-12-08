using UnityEngine;
using Yurowm.Shapes;

namespace YMatchThree.Core {
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteLayerTrigger : LayerTrigger {

        public Sprite sprite;
        
        public override void OnLayerBelow() { }

        public override void OnLayer() {
            var component = GetComponent<SpriteRenderer>();
            component.sprite = sprite;
        }

        public override void OnLayerAbove() { }
    }
}