using UnityEditor;
using Yurowm.ObjectEditors;

namespace Yurowm.Advertising {
    public class AdIntegrationEditor : ObjectEditor<AdIntegration> {

        public override void OnGUI(AdIntegration integration, object context = null) {  
            integration.weight = EditorGUILayout.IntField("Weight", integration.weight);
            integration.typeMask = (AdType) EditorGUILayout.EnumFlagsField("Type Mask", integration.typeMask);
            EditList("App Keys", integration.appKeys);
            EditList("Ad Keys", integration.adKeys);
        }
    }
}