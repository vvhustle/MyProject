using UnityEditor;
using Yurowm.ObjectEditors;

namespace Yurowm.Advertising {
    public class AppKeyEditor : ObjectEditor<AppKey> {
        public override void OnGUI(AppKey key, object context = null) {
            key.platform = (AdPlatform) EditorGUILayout.EnumPopup("Platform", key.platform);
            key.key = EditorGUILayout.TextField("Key", key.key);
        }
    }
}