#if SIGN_IN_WITH_APPLE
using AppleAuth.Editor;
using UnityEditor;
using UnityEditor.iOS;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;

namespace Yurowm.Services {
	public static class SignInWithAppleIntegrationProcessor {
		
		[PostProcessBuild(1)]
		public static void OnPostProcessBuild(BuildTarget target, string path) {
			if (target == BuildTarget.iOS) {
				var projectPath = PBXProject.GetPBXProjectPath(path);
	        
				var project = new PBXProject();
				project.ReadFromString(System.IO.File.ReadAllText(projectPath));
				var manager = new ProjectCapabilityManager(projectPath, "Entitlements.entitlements", null, project.GetUnityMainTargetGuid());
				manager.AddSignInWithApple();
				manager.WriteToFile();
			}
			
			if (target == BuildTarget.StandaloneOSX) {
				AppleAuthMacosPostprocessorHelper.FixManagerBundleIdentifier(target, path);
			}
		}
	}
}
#endif
