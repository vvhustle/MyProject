using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
#if SIGN_IN_WITH_APPLE
using AppleAuth;
using AppleAuth.Enums;
using AppleAuth.Extensions;
using AppleAuth.Interfaces;
using AppleAuth.Native;
#endif
using UnityEngine;
using Yurowm.Core;
using Yurowm.Coroutines;
using Yurowm.Integrations;
using Yurowm.Serialization;
using Yurowm.Utilities;

namespace Yurowm.Services {
    public class SignInWithAppleIntegration : Integration {
        public override string GetName() => "Sign in with Apple";

		public override void Initialize() {
			base.Initialize();
			
			#if SIGN_IN_WITH_APPLE
			if (AppleAuthManager.IsCurrentPlatformSupported) {
				var deserializer = new PayloadDeserializer();
				manager = new AppleAuthManager(deserializer); 
				
				Update().Run();
			}
			#endif
		}
			
		#if SIGN_IN_WITH_APPLE
		IAppleAuthManager manager;

		IEnumerator Update() {
			while (true) {
				manager?.Update();
				yield return null;
			}
		}
		
		public void SignIn(Action<IAppleIDCredential> onSuccess, Action<IAppleError> onFailed) {
			if (manager == null) return;

			var loginArgs = new AppleAuthLoginArgs(LoginOptions.IncludeEmail | LoginOptions.IncludeFullName);

			manager.LoginWithAppleId(
				loginArgs,
				credential => {
					// Obtained credential, cast it to IAppleIDCredential
					var appleID = credential as IAppleIDCredential;
					if (appleID != null) {
						var data = App.data.GetModule<AuthApple>();
						
						data.userID = appleID.User;

						if (appleID.Email != null)
							data.email = appleID.Email;

						if (appleID.FullName != null) 
							data.nickname = appleID.FullName.Nickname;

						data.token = Encoding.UTF8.GetString(
							appleID.IdentityToken,
							0,
							appleID.IdentityToken.Length);

						data.code = Encoding.UTF8.GetString(
							appleID.AuthorizationCode,
							0,
							appleID.AuthorizationCode.Length);
						
						data.SetDirty();

						onSuccess?.Invoke(appleID);
					}
				},
				error => {
					var authorizationErrorCode = error.GetAuthorizationErrorCode();
					
					onFailed?.Invoke(error);
				});
		}
		#endif

		public override bool HasAllNecessarySDK() {
			#if SIGN_IN_WITH_APPLE
			return true;
			#else
			return false;
			#endif
		}

		public bool IsSupportedPlatform() {
			switch (Application.platform) {
				case RuntimePlatform.OSXPlayer:
				case RuntimePlatform.IPhonePlayer:
					return true;
				default:
					return false;
			}
		}
	}

	public class AuthApple: GameData.Module {
		public string code;
		public string token;
		public string nickname;
		public string email;
		public string userID;

		public override void Serialize(Writer writer) {
			writer.Write("code", code);
			writer.Write("token", token);
			writer.Write("nickname", nickname);
			writer.Write("email", email);
			writer.Write("userID", userID);
		}

		public override void Deserialize(Reader reader) {
			reader.Read("code", ref code);
			reader.Read("token", ref token);
			reader.Read("nickname", ref nickname);
			reader.Read("email", ref email);
			reader.Read("userID", ref userID);
		}
	}

	public class SignInWithAppleSymbol : ScriptingDefineSymbolAuto {
		public override string GetSybmol() {
			return "SIGN_IN_WITH_APPLE";
		}

		public override IEnumerable<string> GetRequiredPackageIDs() {
			yield return "com.lupidan.apple-signin-unity";
		}
	}
}