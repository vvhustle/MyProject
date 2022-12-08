using Yurowm.Colors;
using Yurowm.Extensions;

namespace Yurowm.Shapes {
    public class SetMeshAsset : Repaint, IRepaintSetShape {
        IMeshAssetComponent component;
        
        public void SetMesh(MeshAsset meshAsset) {
            if (component != null || this.SetupComponent(out component))
                component.meshAsset = meshAsset;
        }
    }
}