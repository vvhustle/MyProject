using Yurowm.Shapes;

namespace YMatchThree.Core {
    public class ReshapeLayerTrigger : LayerTrigger {

        public MeshAsset meshAsset;
        
        public override void OnLayerBelow() { }

        public override void OnLayer() {
            var component = GetComponent<IMeshAssetComponent>();
            component.meshAsset = meshAsset;
        }

        public override void OnLayerAbove() { }
    }
}