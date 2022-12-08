using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yurowm.Coroutines;
using Yurowm.DebugTools;
using Yurowm.Extensions;
using Yurowm.Serialization;
using Yurowm.Utilities;

namespace Yurowm.Core {
    public static class App {
        
        public static Action onFirstLaunch = delegate {};
        public static Action onLaunch = delegate {};
        public static Action onQuit = delegate {};
        public static Action onFocus = delegate {};
        public static Action onUnfocus = delegate {};

        static GameData _data;
        public static GameData data {
            get => _data;
            private set => _data = value;
        }
        
        static AppBehaviour appBehaviour;

        [OnLaunch(int.MinValue)]
        static IEnumerator StartLaunch() {
            if (OnceAccess.GetAccess("App_Min")) {
                #if UNITY_EDITOR
                data = new GameData("PlayerEditor");
                #else
                data = new GameData("Player", "NixM20TSkARg1ax");
                #endif
                yield return data.Load();
            }
        }
        
        [OnLaunch(int.MaxValue)]
        static void EndLaunch() {
            if (OnceAccess.GetAccess("App_Max")) {
                var appData = data.GetModule<Data>();
                
                if (appData.LaunchCount == 0)
                    onFirstLaunch.Invoke();
                
                appData.OnLaunch();
                onLaunch.Invoke();
                onFocus.Invoke();
                
                appBehaviour = new GameObject("App").AddComponent<AppBehaviour>();
                appBehaviour.gameObject.hideFlags = HideFlags.HideAndDontSave;
                
                UILogic().Run();
            }
        }

        #region UI
        
        public static Action onScreenResize = delegate {};
        
        public static RectOffsetFloat safeOffset {get; private set;}
        public static bool isTablet {get; private set;}
        
        static IEnumerator UILogic() {
            Vector2 screenSize = default;
            ScreenOrientation screenOrientation = default;
            
            isTablet = IsTablet();
            
            while (true) {
                if (screenSize.x != Screen.width 
                    || screenSize.y != Screen.height 
                    || screenOrientation != Screen.orientation) {
                    
                    screenSize.x = Screen.width;
                    screenSize.y = Screen.height;
                    screenOrientation = Screen.orientation;

                    safeOffset = RectOffsetFloat.Delta(
                        new Rect(default, screenSize), 
                        Screen.safeArea);
                    
                    DebugPanel.Log("Safe Area Offset", "UI", safeOffset);
                    DebugPanel.Log("Orientation", "UI", screenOrientation);
                    DebugPanel.Log("Resolution", "UI", screenSize);

                    onScreenResize.Invoke();
                }
                
                yield return null;
            }
        }
        
        static bool IsTablet() {
            var longestSide = YMath.Max(Screen.width, Screen.height);
            
            if (longestSide < 800) 
                return false;

            switch (Application.platform) {
                case RuntimePlatform.Android:
                case RuntimePlatform.IPhonePlayer:
                    var diagonal = Mathf.Sqrt(Mathf.Pow(Screen.width, 2) + Mathf.Pow(Screen.height, 2)) / Screen.dpi;
                    if (diagonal >= 6.5f) 
                        return true;
                    break;
            } 
            
            return false;
        }
        
        #endregion

        class Data : GameData.Module {
            public int LaunchCount {get; private set;}
            
            public void OnLaunch() {
                LaunchCount++;
                SetDirty();
            }

            public override void Serialize(Writer writer) {
                writer.Write("launchCount", LaunchCount);
            }

            public override void Deserialize(Reader reader) {
                LaunchCount = reader.Read<int>("launchCount");
            }
        }
        
        class AppBehaviour : MonoBehaviour {
            void OnApplicationFocus(bool hasFocus) {
                if (hasFocus)
                    onFocus.Invoke();
                else
                    onUnfocus.Invoke();
            }

            void OnApplicationQuit() {
                onQuit.Invoke();    
            }
        }

        #region Go to external
        
        public static void OpenAppStorePage() {            
            if (Application.isEditor) { 
                Application.OpenURL("https://google.com");
                return;
            }
            
            switch (Application.platform) {
                case RuntimePlatform.Android: 
                    Application.OpenURL($"market://details?id={Application.identifier}");
                    return;
                case RuntimePlatform.IPhonePlayer:
                    #if UNITY_IOS
                    UnityEngine.iOS.Device.RequestStoreReview();
                    #endif
                    return;
            }
        }
    
        public static void OpenHelpPopup() {
            if (Application.isEditor) {
                Application.OpenURL("https://gmail.com");
                return;   
            }

            switch (Application.platform) {
                case RuntimePlatform.Android:
                case RuntimePlatform.IPhonePlayer:
                    var email = PropertyStorage.Load<ProjectSettings>().supportEmail;
                    if (email.IsNullOrEmpty())
                        return;
                    
                    var subject = $"{Application.productName} Support";
                    
                    var body = $"Bundle ID: {Application.identifier}\n" 
                               + $"Version: {Application.version}\n" 
                               + $"Platform: {Application.platform}";
                    
                    Application.OpenURL($"mailto:{email}?subject={Uri.EscapeDataString(subject)}&body={Uri.EscapeDataString(body)}");
                    
                    return;
            }
        }

        #endregion
    }
}