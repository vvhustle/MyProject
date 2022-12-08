using UnityEditor;
using YMatchThree.Core;
using Yurowm.ObjectEditors;

namespace YMatchThree.Editor {
    public class LevelScriptOrderedParametersEditor : ObjectEditor<LevelScriptOrdered> {
        public override void OnGUI(LevelScriptOrdered script, object context = null) {
            EditorGUILayout.LabelField("World", script.worldName);
        }
    }
}