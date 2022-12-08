using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if PLAYFAB
using PlayFab;
using PlayFab.AuthenticationModels;
using PlayFab.ClientModels;
using YMatchThree.Meta;
using Yurowm.Features;
using Yurowm.Store;
#endif
using UnityEngine;
using UnityEngine.PlayerLoop;
using YMatchThree.Core;
using Yurowm.ComposedPages;
using Yurowm.Core;
using Yurowm.Coroutines;
using Yurowm.DebugTools;
using Yurowm.Extensions;
using Yurowm.Integrations;
using Yurowm.Serialization;
using Yurowm.UI;
using Yurowm.Utilities;

namespace Yurowm.Services {
    public class PlayfabIntegration : Integration {
        public override string GetName() => "Playfab Core";
		
		AuthData authData => App.data.GetModule<AuthData>();

		bool hasCredentials = false;
		
		public override void Initialize() {
			base.Initialize();
			
			#if PLAYFAB
			
			Update().Run();
			
			GameBinder.BindButtons("SignIn", PlayfabSignInPopup.Show);
			GameBinder.BindButtons("SignOut", SignOut);
			
			DebugPanel.Log("Save Progress", "Playfab", () => 
				SaveProgress(PlayerData.GetRawData()));
			
			DebugPanel.Log("Load Progress", "Playfab", () => 
				LoadProgress(
					keys: PlayerData.GetModuleKeys(),
					onSuccess: data => PlayerData.SetRawData(data)));
			
			SignIn();
			
			#endif
		}

		public override bool HasAllNecessarySDK() {
			#if PLAYFAB
			return true;
			#else
			return false;
			#endif	
		}

		public bool IsLoggedIn() {
			#if PLAYFAB
			return hasCredentials && PlayFabSettings.staticPlayer.IsClientLoggedIn();
			#else
			return false;
			#endif	
		}
			
		#if PLAYFAB
		IEnumerator Update() {
			while (true) {
				DebugPanel.Log("Is Logged In", "Playfab", IsLoggedIn());
				DebugPanel.Log("Playfab ID", "Playfab", PlayFabSettings.staticPlayer.PlayFabId);
				
				yield return new Wait(1);
			}
		}

		#region Sign
		
		public void SignUp(string email, string username, string password, Action onSuccess, Action<string> onFailed) {
			var request = new RegisterPlayFabUserRequest();
			request.Username = username;
			request.Email = email;
			request.Password = password;
			
			void OnSuccess(RegisterPlayFabUserResult result) {
				Debug.Log("Successfully signed up\n" + result.ToJson());
				
				OnSignedIn(result.AuthenticationContext, result.EntityToken.TokenExpiration);
				
				onSuccess?.Invoke();
			}
			
			void OnError(PlayFabError error) {
				OnRequestFailed(error);
				onFailed?.Invoke(error.ErrorMessage);
			}
			
			PlayFabClientAPI.RegisterPlayFabUser(request, 
				OnSuccess, OnError);
		}
		
		public void SignIn(string email, string password, Action onSuccess, Action<string> onFailed) {
			var request = new LoginWithEmailAddressRequest();
			request.Email = email;
			request.Password = password;

			void OnSuccess(LoginResult result) {
				Debug.Log("Successfuly signed in\n" + result.ToJson());
				
				OnSignedIn(result.AuthenticationContext, result.EntityToken.TokenExpiration);
				
				onSuccess?.Invoke();
			}
			
			void OnError(PlayFabError error) {
				OnRequestFailed(error);
				onFailed?.Invoke(error.ErrorMessage);
			}
			
			PlayFabClientAPI.LoginWithEmailAddress(request, 
				OnSuccess, OnError);
		}
		
		public void SignInApple(Action onSuccess, Action<string> onFailed) {
			#if SIGN_IN_WITH_APPLE
		
			var signInWithAppleIntegrarion = Get<SignInWithAppleIntegration>();
			
			if (signInWithAppleIntegrarion == null)
				return;
			
			signInWithAppleIntegrarion.SignIn(
				onSuccess: appleID => {
					var request = new LoginWithAppleRequest();
					request.CreateAccount = true;
					request.IdentityToken = Encoding.UTF8.GetString(appleID.IdentityToken);

					void OnSuccess(LoginResult result) {
						Debug.Log("Successfully signed in\n" + result.ToJson());
						
						OnSignedIn(result.AuthenticationContext, result.EntityToken.TokenExpiration);
						
						onSuccess?.Invoke();
					}
					
					void OnError(PlayFabError error) {
						OnRequestFailed(error);
						onFailed?.Invoke(error.ErrorMessage);
					}
					
					PlayFabClientAPI.LoginWithApple(request, 
						OnSuccess, OnError);
				}, onFailed: error => {
					onFailed?.Invoke(error.LocalizedFailureReason);
				});
			#else
			onFailed?.Invoke("SignInWithApple SDK isn't installed");
			#endif
		}
		
		public void SignInFacebook(Action onSuccess, Action<string> onFailed) {
			#if FACEBOOK
		
			var facebookIntegrarion = Get<FacebookIntegration>();
			
			if (facebookIntegrarion == null)
				return;
			
			facebookIntegrarion.SignIn(
				onSuccess: accessToken => {
					var request = new LoginWithFacebookRequest();
					request.CreateAccount = true;
					request.AccessToken = accessToken;

					void OnSuccess(LoginResult result) {
						Debug.Log("Successfully signed in\n" + result.ToJson());
						
						OnSignedIn(result.AuthenticationContext, result.EntityToken.TokenExpiration);
						
						onSuccess?.Invoke();
					}
					
					void OnError(PlayFabError error) {
						OnRequestFailed(error);
						onFailed?.Invoke(error.ErrorMessage);
					}
					
					PlayFabClientAPI.LoginWithFacebook(request, 
						OnSuccess, OnError);
				}, onFailed: () => 
					onFailed?.Invoke("Canceled by user"));
			#else
			onFailed?.Invoke("SignInWithApple SDK isn't installed");
			#endif
		}
		
		void SignIn() {
			var authData = this.authData;
			
			if (authData.token.IsNullOrEmpty() || authData.tokenExpires <= DateTime.Now)
				return;

			PlayFabSettings.staticPlayer.ClientSessionTicket = authData.ticket;
			PlayFabSettings.staticPlayer.EntityToken = authData.token;
			PlayFabSettings.staticPlayer.PlayFabId = authData.playfabID;
			
			OnSignedIn(PlayFabSettings.staticPlayer);
		}
		
		public void SignOut() {
			if (!IsLoggedIn()) return;
			
			var authData = this.authData;

			authData.ticket = string.Empty;
			authData.token = string.Empty;
			authData.tokenExpires = DateTime.Now;
			authData.playfabID = string.Empty;
			hasCredentials = false;
			
			authData.SetDirty();
			
			PlayFabSettings.staticPlayer.EntityToken = null;
			PlayFabSettings.staticPlayer.ClientSessionTicket = null;
			PlayFabSettings.staticPlayer.PlayFabId = null;
			
			onSignedOut.Invoke();
			
			UIRefresh.Invoke();
		}
		
		public void OnSignedIn(PlayFabAuthenticationContext context, DateTime? tokenExpires = null) {
			var authData = this.authData;

			authData.ticket = context.ClientSessionTicket;
			authData.token = context.EntityToken;
			if (tokenExpires.HasValue)
				authData.tokenExpires = tokenExpires.Value;
			authData.playfabID = context.PlayFabId;
			
			authData.SetDirty();
			
			var requestAccountInfo = new GetAccountInfoRequest();
			requestAccountInfo.PlayFabId = authData.playfabID;
			
			void OnSuccess(GetAccountInfoResult result) {
				authData.username = result.AccountInfo.Username;
				authData.SetDirty();

				hasCredentials = true;
				
				UIRefresh.Invoke();
				
				onSignedIn.Invoke();
			}
			
			PlayFabClientAPI.GetAccountInfo(requestAccountInfo, OnSuccess, OnRequestFailed);
		}
		
		Action onSignedIn = delegate {};
		Action onSignedOut = delegate {};
		
		public void AddOnSignedInListener(Action listener) {
			if (listener == null) return;
			
			onSignedIn += listener;
			
			if (IsLoggedIn())
				listener.Invoke();
		}
		
		public void AddOnSignedOutListener(Action listener) {
			if (listener == null) return;
			
			onSignedOut += listener;
		}
		
		#endregion
		void OnRequestFailed(PlayFabError error) {
			Debug.Log(error.GenerateErrorReport());
		}
		
		public void SaveProgress(Dictionary<string, string> data, Action onSuccess = null, Action onFailed = null) {
			var request = new UpdateUserDataRequest();
			request.Data = data;
			
			void OnSuccess(UpdateUserDataResult result) {
				Debug.Log("Progress saved!");
				onSuccess?.Invoke();
			}
			
			void OnFailed(PlayFabError error) {
				OnRequestFailed(error);
				onFailed?.Invoke();
			}
			
			PlayFabClientAPI.UpdateUserData(request, OnSuccess, OnFailed);
		}
		
		public void LoadProgress(IEnumerable<string> keys, Action<Dictionary<string, string>> onSuccess, Action onFailed = null) {
			var request = new GetUserDataRequest();
			request.Keys = keys.ToList();
			
			void OnSuccess(GetUserDataResult result) {
				Debug.Log("Progress saved!");
				onSuccess?.Invoke(result.Data
					.ToDictionary(p => p.Key, p => p.Value.Value));
			}
			
			void OnFailed(PlayFabError error) {
				OnRequestFailed(error);
				onFailed?.Invoke();
			}
			
			PlayFabClientAPI.GetUserData(request, OnSuccess, OnFailed);
		}

		#endif
		
		[ReferenceValue("IsLoggedIn")]
		static int IsLoggedInReferenceValue() {
			return Get<PlayfabIntegration>()?.IsLoggedIn() ?? false ? 1 : 0;
		}
		
		[ReferenceValue("HasLoginFeature")]
		static int HasLoginFeature() {
			#if PLAYFAB
			return 1;
			#else
			return 0;
			#endif
		}
	}
	
	public class AuthData: GameData.Module {
		public string token = "";
		public string ticket = "";
		public string playfabID = "";
		
		public DateTime tokenExpires = DateTime.Now;
		
		public string username = "Player";

		public override void Serialize(Writer writer) {
			writer.Write("token", token);
			writer.Write("ticket", ticket);
			writer.Write("playfabID", playfabID);
			writer.Write("tokenExpires", tokenExpires);
			
			writer.Write("username", username);
		}

		public override void Deserialize(Reader reader) {
			reader.Read("token", ref token);
			reader.Read("ticket", ref ticket);
			reader.Read("playfabID", ref playfabID);
			reader.Read("tokenExpires", ref tokenExpires);
			
			reader.Read("username", ref username);
		}
	}
	
	public class PlayfabSymbol : ScriptingDefineSymbolAuto {
		public override string GetSybmol() {
			return "PLAYFAB";
		}

		public override IEnumerable<string> GetRequiredNamespaces() {
			yield return "PlayFab";
		}
	}
}