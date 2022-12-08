using Yurowm.ObjectEditors;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

namespace Yurowm.Services {
    public class SignInWithAppleIntegrationEditor : ObjectEditor<SignInWithAppleIntegration> {
        public override void OnGUI(SignInWithAppleIntegration integration, object context = null) {
            if (!integration.HasAllNecessarySDK()) {
                if (GUILayout.Button("Install Package")) {
                    Client.Add("https://github.com/lupidan/apple-signin-unity.git");
                    AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
                }
            }
        }
    }
}