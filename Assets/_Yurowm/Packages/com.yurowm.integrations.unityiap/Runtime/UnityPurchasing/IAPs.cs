using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Yurowm.Extensions;
using Yurowm.Localizations;
using Yurowm.Serialization;
#if UNITY_IAP
using UnityEngine.Purchasing;
#endif

namespace Yurowm.Services {
    public abstract class IAP : ISerializable {
        public string ID;
        public string SKU;
            
        #if UNITY_IAP
        public abstract ProductType GetProductType();
        #endif

        public Action<PurchaseInfo> onSuccess;
            
        public struct PurchaseInfo {
            public string orderID;
        }
        
        const string priceFormat = "price.{0}";
        const string priceValueFormat = "priceValue.{0}";
        const string priceCurrencyFormat = "priceCurrency.{0}";
        public virtual string Price {
            get {
                if (_PriceValue < 0 || _PriceCurrency.IsNullOrEmpty())
                    return "N/A";
                return $"{Math.Round(_PriceValue, 2).ToString(CultureInfo.InvariantCulture)} {_PriceCurrency}";
            }
        }
        
        float _PriceValue = -1;
        public float PriceValue {
            get => _PriceValue;
            set {
                if (_PriceValue == value) return;
                _PriceValue = value;
                GameSettings.Instance.Set(priceValueFormat.FormatText(SKU), _PriceValue.ToString(CultureInfo.InvariantCulture));
            }
        }
        
        string _PriceCurrency = null;
        public string PriceCurrency {
            get => _PriceCurrency;
            set {
                if (_PriceCurrency == value) return;
                _PriceCurrency = value;
                GameSettings.Instance.Set(priceCurrencyFormat.FormatText(SKU), _PriceCurrency);
            }
        }

        public virtual void Serialize(Writer writer) {
            writer.Write("ID", ID);
            writer.Write("SKU", SKU);
        }

        public virtual void Deserialize(Reader reader) {
            reader.Read("ID", ref ID);
            reader.Read("SKU", ref SKU);
        }
    }
        
    public class ConsumableIAP : IAP {
            
        #if UNITY_IAP
        public override ProductType GetProductType() {
            return ProductType.Consumable;
        }
        #endif
    }
        
    public class NonConsumableIAP : IAP {
            
        #if UNITY_IAP
        public override ProductType GetProductType() {
            return ProductType.NonConsumable;
        }
        #endif
    }
        
    public class SubscriptionIAP : IAP {
        
        public enum Period {
            Unknown = 0,
            PT1M = 11,
            PT5M = 15,
            PT15M = 115,
            PT30M = 130,
            P1H = 21,
            P1D = 31,
            P3D = 33,
            P1W = 41,
            P2W = 42,
            P1M = 51,
            P3M = 53,
            P6M = 56,
            P1Y = 61
        }
        
        public SubscriptionIAP() {
            onSuccess += ActivateSubscription;
        }

        public virtual void ActivateSubscription(PurchaseInfo purchaseInfo) {
            Active = true;
        }
        
        public bool Active = false;
        
        #if UNITY_IAP
        public override ProductType GetProductType() {
            return ProductType.Subscription;
        }
        #endif

        public override string Price {
            get {
                if (_PricePeriod == Period.Unknown)
                    return base.Price;
                try { 
                    return Localization.content[periodKeyFormat.FormatText(_PricePeriod)].FormatText(base.Price);
                } catch (Exception) {
                    return base.Price;
                }
            }
        }

        const string pricePeriodFormat = "pricePeriod.{0}";
        
        Period _PricePeriod = Period.Unknown;
        public Period PricePeriod {
            get => _PricePeriod;
            set {
                if (_PricePeriod == value) return;
                _PricePeriod = value;
                GameSettings.Instance.Set(pricePeriodFormat.FormatText(SKU), ((int) _PricePeriod).ToString());
            }
        }
        
        public void SetRawPeriod(string rawPeriod) {
            if (rawPeriod.IsNullOrEmpty())
                PricePeriod = Period.Unknown;
            PricePeriod = Enum.TryParse(rawPeriod, out Period p) ? p : Period.Unknown;
        }
        
        #region Localization
        
        const string periodKeyFormat = "Store/SubscriptionPeriods/{0}";

        [LocalizationKeysProvider]
        static IEnumerator<string> GetLocalizationKeys() {
            foreach (var period in Enum.GetNames(typeof(Period)))
                yield return periodKeyFormat.FormatText(period);
        }
        
        #endregion
    }
}