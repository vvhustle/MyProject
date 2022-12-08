using Yurowm.ObjectEditors;
using UnityEditor;
using YMatchThree.Core;

namespace Yurowm.Editors {
    public class FieldBoosterEditor : ObjectEditor<FieldBooster> {
        public override void OnGUI(FieldBooster booster, object context = null) {
            Edit("Description", booster.descriptionText);
        }
    }
}