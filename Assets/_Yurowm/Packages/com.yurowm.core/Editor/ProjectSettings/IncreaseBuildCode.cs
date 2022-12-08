#if UNITY_ANDROID || UNITY_IOS

using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using Yurowm.Core;
using Yurowm.Serialization;

namespace Yurowm.EditorCore {
        
    public class IncreaseBuildCode : IPreprocessBuildWithReport {
        public int callbackOrder => 0;
        
        public void OnPreprocessBuild(BuildReport report) {
            var buildCode = PlayerSettings.Android.bundleVersionCode;
            buildCode ++;
            
            PlayerSettings.Android.bundleVersionCode = buildCode;
            PlayerSettings.iOS.buildNumber = buildCode.ToString();
            
            var settings = PropertyStorage.Load<ProjectSettings>();
            
            PlayerSettings.bundleVersion = settings.versionName;
            
            settings.buildCode = buildCode;
            
            PropertyStorage.Save(settings);
        }
    }
}

#endif
