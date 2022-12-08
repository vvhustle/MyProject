using UnityEditor;
using Yurowm.ObjectEditors;

namespace Yurowm.Analytics {
    public class FlurryIntegrationEditor : ObjectEditor<FlurryIntegration> {
        public override void OnGUI(FlurryIntegration integration, object context = null) {  
            integration.ApiKey_Android = EditorGUILayout.TextField("Android API Key", integration.ApiKey_Android);
            integration.ApiKey_iOS = EditorGUILayout.TextField("iOS API Key", integration.ApiKey_iOS);
        }
    }
}