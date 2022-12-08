using UnityEditor;
using UnityEngine;
using Yurowm.ObjectEditors;

namespace Yurowm.Advertising {
    public class AdKeyEditor : ObjectEditor<AdKey> {
        
        static GUIContent placeIDtitle = new GUIContent("Place ID", 
            "Leave it empty to use it as default");
        
        public override void OnGUI(AdKey key, object context = null) {
            key.platform = (AdPlatform) EditorGUILayout.EnumPopup("Platform", key.platform);
            key.type = (AdType) EditorGUILayout.EnumPopup("Type", key.type);
            key.key = EditorGUILayout.TextField("Key", key.key);
            key.placeID = EditorGUILayout.TextField( placeIDtitle, key.placeID);
        }
    }
}