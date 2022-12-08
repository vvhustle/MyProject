using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.Purchasing;
using YMatchThree.Meta;
using Yurowm;
using Yurowm.Colors;
using Yurowm.ComposedPages;
using Yurowm.Core;
using Yurowm.Extensions;
using Yurowm.Integrations;
using Yurowm.Localizations;
using Yurowm.Serialization;
using Yurowm.Services;
using Yurowm.UI;

namespace Yurowm.Store {
    public class StorePackRewardedVideo : StorePack {
        
        protected override void SetupBuyButton(StoreGoodButton button) {
            SetAction(button, Localization.content[watchAdsKey],
                Adverts.IsReady(AdType.Rewarded) ? OnClick : (Action) null);
        }

        public override bool IsOwned() {
            var storeData = PlayerData.store;
            return base.IsOwned() || (!unlocks.IsEmpty() && unlocks.All(u => storeData.HasAccess(u)));
        }

        public override void OnClick() {
            Adverts.ShowAds(OnSuccess);
        }
        
        void OnSuccess() {
            Redeem();
        }
        
        #region Localizion

        const string watchAdsKey = "Store/watchAds";
        
        [LocalizationKeysProvider]
        static IEnumerator<string> GetKeys() {
            yield return watchAdsKey;
        }
        
        #endregion
    }
}