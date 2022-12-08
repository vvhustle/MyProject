using Yurowm.ObjectEditors;

namespace YMatchThree.Core {
    public class ShuffleBoosterEditor : ObjectEditor<ShuffleBooster> {
        public override void OnGUI(ShuffleBooster booster, object context = null) {
            Edit("Put Chip Bouncing", booster.putChipBouncing);
        }
    }
}