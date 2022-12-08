using System;
using System.Collections;
using System.Linq;
using YMatchThree.Meta;
using Yurowm.ComposedPages;
using Yurowm.Core;
using Yurowm.Localizations;
using Yurowm.Serialization;

namespace Yurowm.Store {
    public class StorePackCurrency : StorePack {
        
        public string currencyID;
        public int currencyCount;
        
        void Purchase() {
            if (PlayerData.IsSilentMode()) return;
            
            if (IsEnough()) {
                PlayerData.inventory.SpendItems(currencyID, currencyCount);
                PlayerData.SetDirty();
                
                Redeem();
            }
        }

        protected override void SetupBuyButton(StoreGoodButton button) {
            if (currencyCount <= 0) {
                SetAction(button, Localization.content[takeKey], Redeem);
                return;
            }

            SetAction(button, BuildPrice(), IsEnough() ? Purchase : (Action) null);
        }
        
        bool IsEnough() {
            return App.data
                .GetModule<Inventory>()
                .GetItemCount(currencyID) >= currencyCount;
        }

        string BuildPrice() {
            var currency = storage.GetItem<Currency>(x => x.ID == currencyID);
            
            if (currency == null) return null;
            
            return $"{currencyCount}{currency.symbol}";
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
            writer.Write("currencyID", currencyID);
            writer.Write("currencyCount", currencyCount);
        }
        
        public override void Deserialize(Reader reader) {
            base.Deserialize(reader);
            reader.Read("currencyID", ref currencyID);
            reader.Read("currencyCount", ref currencyCount);
        }
        
        #endregion
    }
}