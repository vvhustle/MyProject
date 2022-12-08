using Yurowm.ObjectEditors;
using UnityEditor;
using YMatchThree.Core;

namespace YMatchThree.Editors {
    public class WrappedBoosterEditor : ObjectEditor<WrappedBooster> {
        public override void OnGUI(WrappedBooster booster, object context = null) {
            Edit("Hit", booster.hit);
        }
    }
}