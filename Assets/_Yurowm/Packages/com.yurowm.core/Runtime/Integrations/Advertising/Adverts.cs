using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Yurowm.Advertising;
using Yurowm.Analytics;
using Yurowm.ContentManager;
using Yurowm.Coroutines;
using Yurowm.DebugTools;
using Yurowm.Extensions;
using Yurowm.Serialization;
using Yurowm.UI;
using Yurowm.Utilities;

namespace Yurowm {
    [Flags]
    public enum AdType {
        Interstitial = 1 << 0,
        Rewarded = 1 << 1
    }
    
    public enum AdPlatform {
        Android = 1,
        iOS = 2
    }

    [JITMethodIssueType]
    public static class Adverts {
        
        public static List<AdIntegration> integrations = new List<AdIntegration>();
        
        static IEnumerable<AdIntegration> Integrations(AdType type) {
            return integrations.Where(i => i.active && i.typeMask.HasFlag(type));
        }
        
        static IEnumerable<AdIntegration> ReadyIntegrations(AdType type) {
            return Integrations(type).Where(i => i.IsReady(type));
        }
        
        static Action reward = null;

        [OnLaunch()]
        public static void Initialize() {
            if (OnceAccess.GetAccess("Adverts")) {
                DebugPanel.Log("Show Regular",  "Ads", () => ShowAds(null, null, true));
                DebugPanel.Log("Show Rewarded", "Ads", () => ShowAds(() => Debug.Log("REWARD!"), null, true));

                Update().Run();
                UIUpdate().Run();
                if (Debug.isDebugBuild)
                    Debugger().Run();
            }
        }

        static void GiveReward() {
            if (reward != null) {
                reward.Invoke();
                reward = null;
            }
        }
        
        static Dictionary<AdType, float> lastAdShow = new Dictionary<AdType, float>();

        static IEnumerator UIUpdate() {
            var uiAccess = new DelayedAccess(1);
            
            var rewardedReady = false;
            
            while (true) {
                if (uiAccess.GetAccess()) {
                    if (rewardedReady != IsReady(AdType.Rewarded)) {
                        rewardedReady = !rewardedReady;
                        UIRefresh.Invoke();
                    }
                }

                yield return null;
            }
        }
        
        static IEnumerator Update() {
            var access = new DelayedAccess(10);
            
            while (true) {
                
                if (access.GetAccess()) {

                    foreach (AdIntegration integration in integrations)
                        integration.OnUpdate();
                    
                    foreach (AdType type in Enum.GetValues(typeof(AdType))) {
                        var preload = false;
                        
                        preload |= type == AdType.Rewarded;
                        // preload |= (type == AdType.Regular && !regularIsCharged && preloadAccess.GetAccess());
                        
                        // (Time.unscaledTime - lastAdShow.Get(type) > parameters.preloadDelay)) 
                        
                        if (preload && ReadyIntegrations(type).All(a => a.IsUseless()))
                            Preload(type);
                    }
                }
                
                yield return null;
            }
        }

        static IEnumerator Debugger() {
            DelayedAccess adsUpdate = new DelayedAccess(1);

            while (true) {
                if (adsUpdate.GetAccess() && DebugPanelUI.Instance) {
                    foreach (AdIntegration network in integrations) {
                        if (!network.active) continue;
                        foreach (AdType type in Enum.GetValues(typeof (AdType))) {
                            if (network.typeMask.HasFlag(type)) {
                                var state = network.LogState(type);
                                if (!state.IsNullOrEmpty()) 
                                    DebugPanel.Log($"{network.GetName()} {type}", "Ads", state);
                            }
                        }
                    }
                }

                yield return null;
            }
        }

        public enum Callback {
            None,
            Start,
            Complete
        }
        
        public static void ShowAds(Action reward = null, Action<Callback> callback = null, bool force = false) {

            if (Application.isEditor) {
                if (reward != null) {
                    reward.Invoke();
                    reward = null;
                }
                callback?.Invoke(Callback.Complete);
                return;
            }

            AdType type = reward == null ? AdType.Interstitial : AdType.Rewarded;
            
            AdIntegration target = ReadyIntegrations(type)
                .GetAllMax(i => i.weight)
                .GetRandom();
            
            if (target == null) {
                callback?.Invoke(Callback.None);
                return;
            }
            
            if (!force && type == AdType.Interstitial) {
                var allowed = true;
                
                var regularIsCharged = preloading.ContainsKey(AdType.Interstitial) || IsReady(AdType.Interstitial);

                allowed = allowed && regularIsCharged && GameSettings.Instance.GetModule<AdvertsSettings>().adsEnabled;
                // allowed = allowed && preloadAccess.GetAccess();
                if (!allowed) {
                    callback?.Invoke(Callback.None);
                    return;
                }
            } 
            
            Adverts.reward = reward;
            ShowAds(target, type, callback);
        }

        static Dictionary<AdType, IAdIntegrationPreloadable> preloading = new Dictionary<AdType, IAdIntegrationPreloadable>();
        
        public static void Preload(AdType type) {
            if (ReadyIntegrations(type).Any(i => !(i is IAdIntegrtaionUseless))) return;
            
            if (preloading.TryGetValue(type, out var ad)) {
                if (ad.IsReady(type)) {
                    preloading.Remove(type);
                    return;
                }
                if (ad.IsLoading(type))
                    return;
            }
            
            var settings = GameSettings.Instance.GetModule<AdvertsSettings>();
            
            if (!settings.adsEnabled) {
                
                if (type == AdType.Interstitial) return;
                
                if (type == AdType.Rewarded && !settings.rewardedAdsEnabled) return;
            }
            
            var preloadable = integrations
                .OrderByDescending(i => i.weight)
                .CastIfPossible<IAdIntegrationPreloadable>()
                .FirstOrDefault(p => p.IsReadyToPreload(type));

            if (preloadable == null) return;
            
            preloadable.Preload(type);
            
            preloading[type] = preloadable;
        }
        
        static void ShowAds(AdIntegration integration, AdType type, Action<Callback> callback) {
            DebugPanel.Log("Ad Request", "Ads", $"{integration.GetName()}: {type}");
            ShowingAds(integration, type, callback).Run();
        }

        static IEnumerator ShowingAds(AdIntegration integration, AdType type, Action<Callback> callback) {
            while (Page.IsAnimating)
                yield return null;
            
            if (!integration.IsReady(type)) {
                callback?.Invoke(Callback.None);
                yield break;
            }

            Analytic.Event("Ad_Show", 
                Segment.New("adType", type),
                Segment.New("adService", integration.GetName()));
  
            preloading.Remove(type);

            lastAdShow[type] = Time.unscaledTime;
            
            callback?.Invoke(Callback.Start);
            
            integration.Show(type, GiveReward, callback);
        }

        public static int CountOfReadyAds(AdType type) {
            return Application.isEditor ? 1 : ReadyIntegrations(type).Count();
        }
        
        public static bool IsReady(AdType adType) {
            var adsSettings = GameSettings.Instance.GetModule<AdvertsSettings>();

            switch (adType) {
                case AdType.Interstitial: return adsSettings.adsEnabled && CountOfReadyAds(adType) > 0;
                case AdType.Rewarded: return (adsSettings.adsEnabled || adsSettings.rewardedAdsEnabled) 
                                             && CountOfReadyAds(adType) > 0;
            }
            
            return false;
        }
    }
}