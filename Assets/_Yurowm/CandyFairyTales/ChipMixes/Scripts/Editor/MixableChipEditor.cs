using UnityEditor;
using YMatchThree.Core;
using Yurowm.Help;
using Yurowm.ObjectEditors;

namespace YMatchThree.Editor {
    public class MixableChipEditor : ObjectEditor<IMixableChip> {
        public override void OnGUI(IMixableChip chip, object context = null) {
            chip.MixPriority = EditorGUILayout.IntField("Mix Priority", chip.MixPriority);
            EditorTips.PopLastRectByID("lc.mix.priority");
        }
    }
}