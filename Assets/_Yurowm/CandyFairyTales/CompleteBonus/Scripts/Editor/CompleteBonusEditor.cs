using UnityEditor;
using YMatchThree.Core;
using Yurowm.GUIHelpers;
using Yurowm.Help;
using Yurowm.ObjectEditors;

namespace YMatchThree.Editor {
    public class CompleteBonusEditor : ObjectEditor<CompleteBonus> {
        public override void OnGUI(CompleteBonus bonus, object context = null) {
            using (GUIHelper.Vertical.Start()) {
                EditorGUILayout.PrefixLabel("Feedback");
                using (GUIHelper.IndentLevel.Start()) 
                    Edit(bonus.feedback);
            }
            EditorTips.PopLastRectByID("lc.completebonus.feedback");
        }
    }
}