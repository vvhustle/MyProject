using YMatchThree.Core;
using Yurowm.ObjectEditors;
using Yurowm.Spaces;

namespace YMatchThree.Editor {
    public class WallsEditor : ObjectEditor<Walls> {
        public override void OnGUI(Walls walls, object context = null) {
            BaseTypesEditor.SelectAsset<SlotContentBody>("Vertical Body", walls, nameof(walls.verticalWallBodyName));
            BaseTypesEditor.SelectAsset<SlotContentBody>("Horizontal Body", walls, nameof(walls.horizontalWallBodyName));
            BaseTypesEditor.SelectAsset<SlotContentBody>("Corner Body", walls, nameof(walls.cornerBodyName));
        }
    }
}