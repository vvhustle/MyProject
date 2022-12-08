using Yurowm.ObjectEditors;
using UnityEditor;
using YMatchThree.Core;
using Yurowm.Utilities;

namespace YMatchThree.Editors {
    public class LinedBoosterEditor : ObjectEditor<LinedBooster> {
        public override void OnGUI(LinedBooster booster, object context = null) {
            Edit("Hit", booster.hit);
        }
    }
}