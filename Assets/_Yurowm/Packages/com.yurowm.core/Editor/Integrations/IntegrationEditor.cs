using UnityEditor;
using Yurowm.ObjectEditors;

namespace Yurowm.Integrations {
    public class IntegrationEditor : ObjectEditor<Integration> {

        public override void OnGUI(Integration integration, object context = null) {
            integration.active = EditorGUILayout.Toggle("Active", integration.active);
            if (!integration.HasAllNecessarySDK())
                EditorGUILayout.HelpBox($"{integration.GetName()} SDK isn't installed.", MessageType.Error, false);
        }
        
        
    }
}