using System.Collections;
using System.Linq;
using YMatchThree.Meta;
#if UNITY_IAP
using UnityEngine.Purchasing;
#endif
using Yurowm.Colors;
using Yurowm.ComposedPages;
using Yurowm.Core;
using Yurowm.Integrations;
using Yurowm.Localizations;
using Yurowm.Serialization;
using Yurowm.Services;

namespace Yurowm.Store {
    public class StorePackIAP : StorePack {
        public string iapID;
        
        #if UNITY_IAP
        
        IAP iap;
        
        public IAP GetIAP() {
            if (iap != null)
                return iap;
            
            iap = Integration.Get<UnityIAPIntegration>()?
                .GetIAP(iapID);
            
            if (iap != null)
                iap.onSuccess += OnSuccess;
                
            return iap;
        }

        void Purchase() {
            if (PlayerData.IsSilentMode()) return;
            
            Integration.Get<UnityIAPIntegration>()?.Purchase(GetIAP());
        }
        
        void OnSuccess(IAP.PurchaseInfo purchaseInfo) {
            Redeem();
        }
        
        #endif

        protected override void SetupBuyButton(StoreGoodButton button) {
            #if UNITY_IAP
            var iap = GetIAP();
            var purchasingSettings = GameSettings.Instance.GetModule<PurchasingSettings>();

            if (iap is NonConsumableIAP && purchasingSettings.HasPurchaseSKU(iap.SKU))
                SetAction(button, Localization.content[takeKey].ColorizeUI("IAP"), Redeem);
            else
                SetAction(button, iap.Price.ColorizeUI("IAP"), OnClick);
            #else
            SetAction(button, "ERROR", () => {});
            #endif
        }

        public override void OnClick() {
            #if UNITY_IAP
            var storeData = PlayerData.store;
            if (unlocks.Any(u => storeData.HasAccess(u))) {
                var popup = ComposedPage.ByID("Popup");
                popup.Show(new AcceptPage(
                    Localization.content[messageKey],
                    Localization.content[okKey],
                    Purchase));
            } else
                Purchase();
            #endif
            
        }
        
        #region Localizion

        const string takeKey = "Store/take";
        const string messageKey = "Popups/PackConflict/text";
        const string okKey = "Popups/PackConflict/ok";

        public override IEnumerable GetLocalizationKeys() {
            yield return base.GetLocalizationKeys();
            yield return takeKey;
            yield return messageKey;
            yield return okKey;
        }

        #endregion
        
        #region ISerializable

        public override void Serialize(Writer writer) {
            base.Serialize(writer);
            writer.Write("iapID", iapID);
        }
        
        public override void Deserialize(Reader reader) {
            base.Deserialize(reader);
            reader.Read("iapID", ref iapID);
        }

        #endregion
    }
}