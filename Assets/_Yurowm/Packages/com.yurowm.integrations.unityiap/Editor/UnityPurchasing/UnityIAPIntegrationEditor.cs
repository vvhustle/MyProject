using System;
using System.Linq;
using UnityEditor;
using Yurowm.Extensions;
using Yurowm.GUIHelpers;
using Yurowm.Integrations;
using Yurowm.ObjectEditors;

namespace Yurowm.Services {
    public class UnityIAPIntegrationEditor : ObjectEditor<UnityIAPIntegration> {

        public override void OnGUI(UnityIAPIntegration integration, object context = null) {
            EditList("IAPs", integration.iaps);
        }
        
        public static void SelectIAPID<I>(string label, string IAP_ID, Action<string> set) where I : IAP {
                
            var intergration = Integration.Get<UnityIAPIntegration>();
            
            if (intergration != null)
                GUIHelper.Popup(label, IAP_ID, 
                    intergration.iaps.CastIfPossible<I>().Select(i => i.ID), 
                    EditorStyles.popup, 
                    id => id, 
                    set);
            else {
                var newID = EditorGUILayout.TextField(label, IAP_ID);
                if (newID != IAP_ID) set(newID);
            }
        }
    }
    
    public class UnityIAPIntegrationEditor_IAPEditor : ObjectEditor<IAP> {
        static readonly string[] emptyPopup = {"Unavaliable"};

        public override void OnGUI(IAP iap, object context = null) {
            iap.ID = EditorGUILayout.TextField("ID", iap.ID);
            iap.SKU = EditorGUILayout.TextField("SKU", iap.SKU);
        }
    }
}