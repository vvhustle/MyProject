using System;
using System.Collections.Generic;
using Yurowm.Integrations;
using Yurowm.Utilities;
#if FACEBOOK
using Facebook.Unity;
using AccessToken = Facebook.Unity.AccessToken;
#endif

namespace Yurowm.Services {
    public class FacebookIntegration : Integration {
        public override string GetName() => "Facebook";

		public override void Initialize() {
			base.Initialize();

			#if FACEBOOK
			if (!FB.IsInitialized)
				FB.Init(() => FB.ActivateApp());
			else
				FB.ActivateApp();
			#endif
		}

		public override bool HasAllNecessarySDK() {
			#if FACEBOOK
			return true;
			#else
			return false;
			#endif
		}

		public void SignIn(Action<string> onSuccess, Action onFailed) {
			#if FACEBOOK
			var perms = new List<string>() {
				"public_profile",
				"email"
			};
			
			void OnCallback (ILoginResult result) {
				if (FB.IsLoggedIn)
					onSuccess?.Invoke(AccessToken.CurrentAccessToken.TokenString);
				else
					onFailed?.Invoke();
			}

			FB.LogInWithReadPermissions(perms, OnCallback);
			#else
			
			onFailed?.Invoke();
			
			#endif
		}
	}
	
	public class FacebookSymbol : ScriptingDefineSymbolAuto {
		public override string GetSybmol() {
			return "FACEBOOK";
		}

		public override IEnumerable<string> GetRequiredNamespaces() {
			yield return "Facebook.Unity";
		}
	}
}