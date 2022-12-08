using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
#if UNITY_IAP_OBFUSCATION
using UnityEngine.Purchasing.Security;
#endif
#if UNITY_IAP
using UnityEngine.Purchasing;
using Yurowm.DebugTools;
using Yurowm.Core;
#endif
using Yurowm.Analytics;
using Yurowm.DebugTools;
using Yurowm.Extensions;
using Yurowm.Integrations;
using Yurowm.Serialization;
using Yurowm.Utilities;
using Object = System.Object;

namespace Yurowm.Services {
    public class UnityIAPIntegration : Integration {
        
        public override string GetName() {
            return "Unity IAP";
        }

        #region IAPs
        
        public List<IAP> iaps = new List<IAP>();
        
        public IAP GetIAP(string id) {
            return iaps.FirstOrDefault(i => i.ID == id);
        }

        #endregion

        #region Initialization
        
        #if UNITY_IAP_OBFUSCATION
        public StoreListener storeListener;
        #endif
        
        #if UNITY_IAP
        public IStoreController storeController;
        public IExtensionProvider storeExtensionProvider;
        
        public Action<PurchaseFailureReason> onPurchaseFailed = null;
        
        public Action<Product> onPurchase = delegate {};
        
        #endif
        
        public override void Initialize() {
            #if UNITY_IAP
            onGratitude += () => Debug.Log("Thank You!");
            
            InitializePurchasing();
            #endif
        }
        
        bool IsInitialized() {
            #if UNITY_IAP_OBFUSCATION
            return storeController != null && storeExtensionProvider != null;
            #else
            return false;
            #endif
        }
        
        void InitializePurchasing() {
            #if UNITY_IAP_OBFUSCATION
            if (IsInitialized())
                return;

            ConfigurationBuilder builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

            foreach (var iap in iaps)
                builder.AddProduct(iap.SKU, iap.GetProductType(), new IDs {
                        {iap.SKU, AppleAppStore.Name},
                        {iap.SKU, GooglePlay.Name}
                    }
                );

            storeListener = new StoreListener(this);
            
            UnityPurchasing.Initialize(storeListener, builder);

            #endif
        }
        
        public void OnInitialize(Action action) {
            if (onInitialize == null)
                action.Invoke();
            else
                onInitialize += action;
        }
            
        
        Action onInitialize = delegate {};

        #endregion

        #region Purchasing & Prising
        
        public Action<bool> onRestore = delegate {};
        public Action<bool> onProcessingStatusChanged = delegate {};
        public Action onPricesUpdate = delegate {};

        #if UNITY_IAP_OBFUSCATION
        void UpdatePrices() {
            if (storeController == null) return;

            foreach (var iap in iaps) {
                Product product = storeController.products.WithID(iap.SKU);
                if (product == null) continue;

                iap.PriceValue = (float) product.metadata.localizedPrice;
                iap.PriceCurrency = product.metadata.isoCurrencyCode;
                
                if (iap is SubscriptionIAP siap) {
                    #if UNITY_EDITOR
                    siap.PricePeriod = SubscriptionIAP.Period.PT5M;
                    #elif UNITY_ANDROID
                    siap.SetRawPeriod(product.metadata.GetGoogleProductMetadata()?.subscriptionPeriod);
                    #endif
                }
            }
            
            onPricesUpdate.Invoke();
        }
        
        internal void OnInitialize() {
            if (onInitialize == null)
                return;
            
            UpdatePrices();
            CheckSubscriptions();
            
            onInitialize.Invoke();
            onInitialize = null;
        }
        
        void CheckSubscriptions() {
            var settings = GameSettings.Instance.GetModule<PurchasingSettings>();
            
            var activeSubscriptions = settings.ActiveSubscriptions().ToList();
            
            var validator = ObfuscatorHelper.GetValidator();
            
            foreach (var siap in iaps.CastIfPossible<SubscriptionIAP>()) {
                if (activeSubscriptions.Any(s => s.SKU == siap.SKU))
                    continue;
                
                var product = storeController.products.WithID(siap.SKU);                
            
                if (product == null || !product.hasReceipt) continue;
                
                var receipts = validator.Validate(product.receipt);

                foreach (var receipt in receipts) {
                    var info = GetSubscriptionInfo(receipt);
                    if (info == null) continue;
                    activeSubscriptions.Add(info);
                    settings.RegisterSubscription(info);
                }
            }
            
            activeSubscriptions
                .Select(s => iaps.CastIfPossible<SubscriptionIAP>().FirstOrDefault(i => i.SKU == s.SKU))
                .NotNull()
                .ForEach(i => i.Active = true);
            
            DebugPanel.Log("Active Subscriptions", () => {
                Debug.Log(settings.ActiveSubscriptions().Select(s => s.ToString()).Join("\n"));
            });
        }
        
        public PurchasingSettings.Subscription GetSubscriptionInfo(IPurchaseReceipt receipt) {
            switch (receipt) {
                case GooglePlayReceipt r when r.purchaseState == GooglePurchaseState.Purchased: {
                    var s = new PurchasingSettings.SubscriptionOneSession();
                    s.SKU = receipt.productID;
                    s.activeSessoin = true;
                    return s;
                }
                case AppleInAppPurchaseReceipt r: {
                    if (r.subscriptionExpirationDate > DateTime.Now) {
                        var s = new PurchasingSettings.SubscriptionExpirationDate();
                        s.SKU = receipt.productID;
                        s.expirationDate = r.subscriptionExpirationDate;
                        return s;
                    }
                } break;
            }
            return null;
        }
        
        #endif
        
        public void Purchase(IAP iap) {
            #if UNITY_IAP
            if (iap == null) return;
            
            Analytic.Event("IAP_Purchase", Segment.New("SKU", iap.SKU));

            if (iap.onSuccess == null) {
                Debug.LogError("IAP Reward is empty");
                return;
            }
                
            try {
                if (IsInitialized()) {
                    Product product = storeController.products.WithID(iap.SKU);
                    if (product != null && product.availableToPurchase) {
                        if (iap is NonConsumableIAP && !product.transactionID.IsNullOrEmpty()) {
                            var purchasingSettings = GameSettings.Instance.GetModule<PurchasingSettings>();
                            if (purchasingSettings.HasPurchase(product.transactionID)) {
                                gratitude = true;
                                Success(iap, new IAP.PurchaseInfo {
                                    orderID = product.transactionID
                                });
                                return;
                            }
                        }
                                
                        Debug.Log("Purchasing product asychronously:'" + product.definition.id + "'");
                            
                        gratitude = true;
                        onProcessingStatusChanged.Invoke(true);
                        storeController.InitiatePurchase(product);
                    } else {
                        Debug.Log("BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");
                    }
                } else {
                    Debug.Log("BuyProductID FAIL. Not initialized.");
                    InitializePurchasing();
                }
            } catch (Exception e) {
                Debug.Log("BuyProductID: FAIL. Exception during purchase. " + e);
            }
            #endif
        }
        
        public void RestorePurchases() {
            #if UNITY_IAP_OBFUSCATION
            if (Application.platform == RuntimePlatform.IPhonePlayer) {
                gratitude = true;
                storeExtensionProvider
                    .GetExtension<IAppleExtensions>()
                    .RestoreTransactions(onRestore);
                return;
            }
            
            onRestore.Invoke(false);
            #endif
        }
        
        public void Success(IAP iap, IAP.PurchaseInfo purchaseInfo) {
            Analytic.Event("IAP_Purchased", Segment.New("SKU", iap.SKU));
            iap.onSuccess?.Invoke(purchaseInfo);
            Gratitude();
        }
        
        #endregion

        #region Gratitude

        public Action onGratitude = null;
        
        bool gratitude = false;
        
        void Gratitude() {
            if (gratitude) {
                onGratitude?.Invoke();
                gratitude = false;
            }
        }

        #endregion

        #region ISerializable

        public override void Serialize(Writer writer) {
            base.Serialize(writer);
            writer.Write("iaps", iaps.ToArray());
        }

        public override void Deserialize(Reader reader) {
            base.Deserialize(reader);
            
            iaps.Clear();
            iaps.AddRange(reader.ReadCollection<IAP>("iaps"));
        }

        #endregion
        

    }

    #if UNITY_IAP_OBFUSCATION
    public class StoreListener : IStoreListener {
        
        UnityIAPIntegration integration;

        public StoreListener(UnityIAPIntegration integration) {
            this.integration = integration;
        }
        
        public void OnInitializeFailed(InitializationFailureReason error) {
            Debug.LogError("Unity IAP Purchasing initializing is failed: " + error);
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e) {
            
            integration.onProcessingStatusChanged.Invoke(false);
            
            bool validPurchase = false;
            var iap = integration.GetIAP(e.purchasedProduct.definition.id);
            
            if (iap == null) return PurchaseProcessingResult.Complete;
            
            if (Application.isEditor) {
                switch (iap) {
                    case SubscriptionIAP _: {
                        var purchasingSettings = GameSettings.Instance.GetModule<PurchasingSettings>();
                        purchasingSettings.RegisterSubscription(new PurchasingSettings.SubscriptionOneSession {
                            SKU = iap.SKU,
                            activeSessoin = true
                        });
                        break;
                    }
                }
                validPurchase = true;
            } else {
                var validator = ObfuscatorHelper.GetValidator();
                
                try {
                    // On Google Play, result has a single product ID.
                    // On Apple stores, receipts contain multiple products.
                    var result = validator.Validate(e.purchasedProduct.receipt);
                    // For informational purposes, we list the receipt(s)
                    Debug.Log("Receipt is valid. Contents:");
                    
                    var purchasingSettings = GameSettings.Instance.GetModule<PurchasingSettings>();
                    foreach (IPurchaseReceipt productReceipt in result) {
                        Debug.Log(productReceipt.productID);
                        Debug.Log(productReceipt.purchaseDate);
                        Debug.Log(productReceipt.transactionID);
                        
                        if (iap.SKU == productReceipt.productID) {
                            switch (iap) {
                                case ConsumableIAP _:
                                    validPurchase = true;
                                    break;
                                
                                case NonConsumableIAP _ 
                                    when purchasingSettings.OwnPurchase(productReceipt.transactionID, iap.SKU): 
                                    validPurchase = true;
                                    break;

                                case SubscriptionIAP _: {
                                    var info = integration.GetSubscriptionInfo(productReceipt);
                                    if (info != null) {
                                        purchasingSettings.RegisterSubscription(info);    
                                        validPurchase = true;
                                    }
                                } break;
                            }
                        }
                    }
                } catch (IAPSecurityException) {
                    Debug.Log("Invalid receipt, not unlocking content");
                }
            }

            if (validPurchase) {
                integration.onPurchase(e.purchasedProduct);
                integration.Success(iap, new IAP.PurchaseInfo {
                    orderID = e.purchasedProduct.transactionID
                });
            }

            return PurchaseProcessingResult.Complete;
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason p) {
            integration.onProcessingStatusChanged.Invoke(false);
            
            var iap = integration.GetIAP(product.definition.id);
            
            if (iap == null) return;
            
            if (p == PurchaseFailureReason.DuplicateTransaction && iap is NonConsumableIAP) {
                integration.Success(iap, new IAP.PurchaseInfo {
                    orderID = product.transactionID
                });
                return;
            }
            
            integration.onPurchaseFailed?.Invoke(p);
            Debug.Log("Purchase failed: " + p);
        }

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions) {
            Debug.Log("Unity IAP Purchasing is initialized");

            integration.storeController = controller;
            integration.storeExtensionProvider = extensions;
            
            integration.OnInitialize();
        }
    }
    
    #endif
    
    
    public class PurchasingSettings : SettingsModule {
        List<string> purchases = new List<string>();
        List<string> purchasesSKU = new List<string>();
        List<Subscription> subscriptions = new List<Subscription>();
        
        /// <summary>
        /// Если такая покупка уже есть, то не пускаем. Если нет, то пускаем - это что-то новое.
        /// </summary>
        /// <param name="transactionId"></param>
        /// <param name="sku"></param>
        /// <returns></returns>
        public bool OwnPurchase(string transactionId, string sku) {
            if (HasPurchase(transactionId) | HasPurchaseSKU(sku))
                return false;
            
            purchasesSKU.Add(sku);
            purchases.Add(transactionId);
            SetDirty();
            return true;
        }

        public bool HasPurchase(string transactionId) {
            return purchases.Contains(transactionId);
        }
        
        public bool HasPurchaseSKU(string sku) {
            return purchasesSKU.Contains(sku);
        }

        public void RegisterSubscription(Subscription subscription) {
            if (subscription == null) 
                return;
            subscriptions.RemoveAll(s => s.SKU == subscription.SKU);
            subscriptions.Add(subscription);
            SetDirty();
        }

        public bool ValidateSubscription(string SKU) {
            return ActiveSubscriptions().Any(s => s.SKU == SKU);
        }
        
        public IEnumerable<Subscription> ActiveSubscriptions() {
            return subscriptions.Where(s => s.IsActive());
        }
        
        public abstract class Subscription : ISerializable {
            public string SKU;

            public abstract bool IsActive();

            public virtual void Serialize(Writer writer) {
                writer.Write("SKU", SKU);
            }

            public virtual void Deserialize(Reader reader) {
                reader.Read("SKU", ref SKU);
            }
        }
        
        [SerializeShort]
        public class SubscriptionOneSession : Subscription {
            public bool activeSessoin = false;
            
            public override bool IsActive() {
                return activeSessoin;
            }

            public override string ToString() {
                return $"{SKU} (one session)";
            }
        }
        
        [SerializeShort]
        public class SubscriptionExpirationDate : Subscription {
            public DateTime expirationDate;

            public override bool IsActive() {
                return expirationDate > DateTime.Now;
            }
            
            public override string ToString() {
                return $"{SKU} ({expirationDate})";
            }
            
            public override void Serialize(Writer writer) {
                base.Serialize(writer);
                writer.Write("expirationDate", expirationDate.Ticks);
            }

            public override void Deserialize(Reader reader) {
                base.Deserialize(reader);
                expirationDate = new DateTime(reader.Read<long>("expirationDate"));
            }
        }
        
        public override void Serialize(Writer writer) {
            writer.Write("purchases", purchases.ToArray());
            writer.Write("purchasesSKU", purchasesSKU.ToArray());
            writer.Write("subscriptions", ActiveSubscriptions().ToArray());
        }

        public override void Deserialize(Reader reader) {
            purchases.Clear();
            purchases.AddRange(reader.ReadCollection<string>("purchases"));
            
            purchasesSKU.Clear();
            purchasesSKU.AddRange(reader.ReadCollection<string>("purchasesSKU"));
            
            subscriptions.Clear();
            subscriptions.AddRange(reader.ReadCollection<Subscription>("subscriptions"));
        }
    }

    public class UnityIAPSymbol : ScriptingDefineSymbolAuto {
        public override string GetSybmol() {
            return "UNITY_IAP";
        }

        public override IEnumerable<string> GetRequiredPackageIDs() {
            yield return "com.unity.purchasing";
        }
    }
}