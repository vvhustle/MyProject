using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Yurowm.Coroutines;
using Yurowm.DebugTools;
using Yurowm.Extensions;
using Yurowm.Serialization;
using Yurowm.Utilities;
#if ADMOB
using GoogleMobileAds.Api;
#endif

namespace Yurowm.Advertising {
    public class AdMobIntegration : AdIntegration, IAdIntegrationPreloadable {

        #if ADMOB
        Dictionary<AdType, Ad> ads = new Dictionary<AdType, Ad>();
        
        public override string LogState(AdType type) {
            if (ads.TryGetValue(type, out var ad))
                return ad.LogState();
            return "N/A";
        }
        #endif
        
        public override void Initialize() {
            base.Initialize();
            
            if (typeMask == 0 || !active) return;
            
            #if ADMOB
            
            IEnumerable<AdKey> keys;
            
            #if DEBUG
            keys = GetTestAdKeys();
            #else
            keys = adKeys;
            #endif

            var platform = GetPlatform();

            foreach (var key in keys) {
                if (key.platform != platform) continue;
                if (!typeMask.OverlapFlag(key.type)) continue;
                
                ads.Add(key.type, new Ad(key.type, key.key));
            }

            MobileAds.Initialize(status => {});
                
            #else
            active = false;
            #endif
        }
        
        IEnumerable<AdKey> GetTestAdKeys() {
            yield return new AdKey {
                platform = AdPlatform.Android,
                type = AdType.Interstitial,
                key = "ca-app-pub-3940256099942544/1033173712"
            };
            yield return new AdKey {
                platform = AdPlatform.Android,
                type = AdType.Rewarded,
                key = "ca-app-pub-3940256099942544/5224354917"
            };
            yield return new AdKey {
                platform = AdPlatform.iOS,
                type = AdType.Interstitial,
                key = "ca-app-pub-3940256099942544/4411468910"
            };
            yield return new AdKey {
                platform = AdPlatform.iOS,
                type = AdType.Rewarded,
                key = "ca-app-pub-3940256099942544/1712485313"
            };
        }

        public override bool HasAllNecessarySDK() {
            #if !ADMOB
            return false;
            #endif
            return base.HasAllNecessarySDK();
        }

        #region IAdIntegrationPreloadable

        public bool IsReadyToPreload(AdType type) {
            #if ADMOB
            if (ads.TryGetValue(type, out var ad))
                return ad.ReadyToRequest();
            #endif
            return false;
        }

        public void Preload(AdType type) {
            #if ADMOB
            if (ads.TryGetValue(type, out var ad))
                ad.Request();
            #endif
        }

        public bool IsLoading(AdType type) {
            #if ADMOB
            if (ads.TryGetValue(type, out var ad))
                return ad.HasState(AdState.Loading);
            #endif
            return false;
        }

        #endregion
        

        public override bool IsReady(AdType type) {
            #if ADMOB
            
            if (ads.TryGetValue(type, out var ad))
                return ad.HasState(AdState.Ready);
            
            #endif
            
            return false;
        }

        public override void Show(AdType type, Action onComplete, Action<Adverts.Callback> callback) {
            #if ADMOB
            if (ads.ContainsKey(type))
                ads[type].Show(onComplete, callback);
            #endif
        }

        public override string GetName() {
            return "Google AdMob";
        }
        
        [Flags]
        enum AdState {
            NotInitialized = 1 << 0,
            Initialized = 1 << 1,
            Loading = 1 << 2,
            Ready = 1 << 3,
            Requested = Loading | Ready,
            Complete = 1 << 4,
            Failed = 1 << 5
        }
        
        #if ADMOB
        
        class Ad {
            string zoneID;
            public readonly AdType adType;
            object core = null;
            
            DateTime suspendUntil;
            
            AdState state = AdState.NotInitialized;
            
            void SetState(AdState s, bool value) {
                if (value)
                    state = state | s;
                else
                    state = state & ~s;
            }
            
            public Ad(AdType adType, string zoneID) {
                this.adType = adType;
                this.zoneID = zoneID;
                BuildCore();
            }
            
            void BuildCore() {
                switch (adType) {
                    case AdType.Interstitial: {
                        var ad = new InterstitialAd(zoneID);
                        ad.OnAdLoaded += OnAdLoaded;
                        ad.OnAdClosed += OnAdClosed;
                        ad.OnAdFailedToLoad += (s, e) => OnAdFailed(e.Message);
                        core = ad;
                        state = AdState.Initialized;
                        return;
                    }
                    case AdType.Rewarded: {
                        var ad = new RewardedAd(zoneID);
                        ad.OnAdLoaded += OnAdLoaded;
                        ad.OnAdClosed += OnAdClosed;
                        ad.OnAdFailedToLoad += (s, e) => OnAdFailed(e.Message);
                        ad.OnUserEarnedReward += OnAdRewarded;
                        core = ad;
                        state = AdState.Initialized;
                        return;
                    }
                    default: {
                        core = null;
                        state = AdState.NotInitialized;
                        return;
                    }
                }
            }
            
            public bool ReadyToRequest() {
                if (HasState(AdState.Failed)) {
                    if (suspendUntil <= DateTime.Now)
                        SetState(AdState.Failed, false);
                    else 
                        return false;
                }

                return HasState(AdState.Initialized) && !HasState(AdState.Requested);
            }
            
            public void Request() {
                
                if (!ReadyToRequest()) return;
                
                Debug.Log($"AdMob Ad Request: {adType}");
                
                switch (adType) {
                    case AdType.Interstitial: (core as InterstitialAd)?.LoadAd(BuildRequest()); break;
                    case AdType.Rewarded: {
                        if (core == null) BuildCore();
                        (core as RewardedAd).LoadAd(BuildRequest()); break;
                    }
                    default: return;
                }
                
                SetState(AdState.Ready, false);
                SetState(AdState.Loading, true);
                SetState(AdState.Complete, false);
            }
            
            void Destroy() {
                switch (adType) {
                    case AdType.Interstitial: (core as InterstitialAd)?.Destroy(); break;
                    case AdType.Rewarded: core = null; break;
                    default: return;
                }
                
                state = AdState.Initialized;
            }
            
            public bool HasState(AdState state) {
                return this.state.HasFlag(state);
            }
            
            void OnAdLoaded(object sender, EventArgs e) {
                SetState(AdState.Ready, true);
                SetState(AdState.Loading, false);
            }

            Action<Adverts.Callback> callback = null;
            
            void OnAdClosed(object sender, EventArgs e) {
                SetState(AdState.Complete, true);
                callback?.Invoke(Adverts.Callback.Complete);
                suspendDelay = 1;
                Destroy();
            }
            
            float suspendDelay = 1;
            
            void OnAdFailed(string message) {
                Debug.LogError(message);
                
                suspendUntil = DateTime.Now.AddMinutes(suspendDelay);
                suspendDelay ++;
                
                callback?.Invoke(Adverts.Callback.Complete);
                
                Destroy();
                SetState(AdState.Failed, true);
            }

            Action getReward = null;
            void OnAdRewarded(object sender, Reward e) {
                if (getReward != null) {
                    getReward.RunInMainThread(wait: false);
                    getReward = null;
                }
            }

            AdRequest BuildRequest() {
                return new AdRequest.Builder().Build();
            }

            public void Show(Action getReward, Action<Adverts.Callback> callback) {
                this.callback = callback;
                switch (core) {
                    case InterstitialAd ad: ad.Show(); break;
                    case RewardedAd ad: {
                        this.getReward = getReward;
                        ad.Show(); 
                        break;
                    }
                }
            }

            public string LogState() {
                return state.ToText();
            }
        }
        #endif
    }
    
    public class AdMobSymbol : ScriptingDefineSymbolAuto {
        public override string GetSybmol() {
            return "ADMOB";
        }

        public override IEnumerable<string> GetRequiredNamespaces() {
            yield return "GoogleMobileAds.Api";
        }
    }
}
