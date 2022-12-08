using Yurowm.ObjectEditors;
using UnityEditor;
using YMatchThree.Core;
using Yurowm.Help;

namespace Yurowm.Editors {
    public class LollipopBoosterEditor : ObjectEditor<LollipopBooster> {
        public override void OnGUI(LollipopBooster booster, object context = null) {
            Edit("Explosion", booster.hitBouncing);
            EditorTips.PopLastRectByID("lc.lollipop.bouncing");
        }
    }
}