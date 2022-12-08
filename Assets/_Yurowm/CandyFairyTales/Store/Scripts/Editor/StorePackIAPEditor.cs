using UnityEditor;
using UnityEngine;
using Yurowm.ObjectEditors;
using Yurowm.Services;

namespace Yurowm.Store {
    public class StorePackIAPEditor : ObjectEditor<StorePackIAP> {
        public override void OnGUI(StorePackIAP item, object context = null) {
            #if UNITY_IAP
            UnityIAPIntegrationEditor.SelectIAPID<IAP>("IAP", item.iapID, id => item.iapID = id);
            #else
            EditorGUILayout.HelpBox("Unity IAP isn't integrated", MessageType.Error, false);
            #endif
            item.savings = EditorGUILayout.FloatField("Savings", item.savings).Clamp01();
        }
    }
}