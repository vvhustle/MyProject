#if UNITY_ADS && (UNITY_ANDROID || UNITY_IOS)
#define UNITY_ADS_INTEGRATION 
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Advertisements;
using Yurowm.Serialization;
using Yurowm.Utilities;
using Yurowm.YDebug;

#if UNITY_ADS_INTEGRATION
using UnityEngine.Advertisements;
#endif

namespace Yurowm.Advertising {
    public class UnityAdsIntegration : AdIntegration {
        
        public override void Initialize() {
            base.Initialize();
            
            if (typeMask == 0 || !active) return;
            
            #if UNITY_ADS_INTEGRATION
            var appKey = GetAppKey();

            if (appKey == null) {
                active = false;
                return;
            }

            Advertisement.AddListener(new Listener(this));
            Advertisement.Initialize(appKey.key);
            #endif
        }

        public override string GetName() {
            return "UnityAds";
        }

        public override string LogState(AdType type) {
            #if UNITY_ADS_INTEGRATION
            return $"{Advertisement.GetPlacementState(GetAdKey(type)?.key)} {IsReady(type)}";
            #else
            return null;
            #endif
        }

        public override bool IsReady(AdType type) {
            #if UNITY_ADS_INTEGRATION
            return Advertisement.IsReady(GetAdKey(type)?.key);
            #else
            return false;
            #endif
        }

        public override void Show(AdType type, Action onComplete, Action<Adverts.Callback> callback) {
            #if UNITY_ADS_INTEGRATION
            Advertisement.Show(GetAdKey(type)?.key, new ShowOptions {
                resultCallback = result => {
                    callback?.Invoke(Adverts.Callback.Complete);
                    if (result == ShowResult.Finished)
                        onComplete?.Invoke();
                }
            });
            #endif
        }

        public override bool HasAllNecessarySDK() {
            #if !UNITY_ADS
            return false;
            #endif
            return base.HasAllNecessarySDK();
        }

        #region IUnityAdsListener 
        
        class Listener : IUnityAdsListener {
            UnityAdsIntegration integration;
            
            public Listener(UnityAdsIntegration integration) {
                this.integration = integration;
            }

            public void OnUnityAdsReady(string placementId) {
                Debug.Log($"OnUnityAdsReady: {placementId}");
            }

            public void OnUnityAdsDidError(string message) {
                Debug.LogError(message);
            }

            public void OnUnityAdsDidStart(string placementId) {
                Debug.Log($"OnUnityAdsDidStart: {placementId}");
            }

            public void OnUnityAdsDidFinish(string placementId, ShowResult showResult) {
                Debug.Log($"OnUnityAdsDidFinish: {placementId} ({showResult})");
            }
        }
        
        #endregion
    }
    
    public class UnityAdsSymbol : ScriptingDefineSymbolAuto {
        public override string GetSybmol() {
            return "UNITY_ADS";
        }

        public override IEnumerable<string> GetRequiredPackageIDs() {
            yield return "com.unity.ads";
        }
    }
}
