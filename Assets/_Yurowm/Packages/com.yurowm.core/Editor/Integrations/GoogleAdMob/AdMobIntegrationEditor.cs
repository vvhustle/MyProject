using UnityEditor;
using Yurowm.Integrations;
using Yurowm.ObjectEditors;

namespace Yurowm.Advertising {
    public class AdMobIntegrationEditor : ObjectEditor<AdMobIntegration> {
            
        public override void OnGUI(AdMobIntegration integration, object context = null) {
            EditorGUILayout.HelpBox(
                "Provide App Keys in Google AdMob settings: Assets / Google Mobile Ads / Settings...\n" +
                "The App Keys provided here will not be used!", MessageType.Warning, false);
        }
    }
}