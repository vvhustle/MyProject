using Yurowm.ObjectEditors;

namespace YMatchThree.Core {
    public class LevelBoosterEditor : ObjectEditor<LevelBooster> {
        public override void OnGUI(LevelBooster booster, object context = null) {
            Edit("Title", booster.titleText);
            Edit("Description", booster.descriptionText);
        }
    }
}