using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using YMatchThree.Meta;
using Yurowm.Colors;
using Yurowm.ComposedPages;
using Yurowm.ContentManager;
using Yurowm.Core;
using Yurowm.Coroutines;
using Yurowm.Extensions;
using Yurowm.Localizations;
using Yurowm.Serialization;
using Yurowm.Spaces;
using Yurowm.UI;
using Yurowm.Utilities;
using Page = Yurowm.ComposedPages.Page;

namespace Yurowm.Store {
    public abstract class StoreItem : ISerializableID, IStorageElementExtraData {
        
        [PreloadStorage]
        public static Storage<StoreItem> storage = new Storage<StoreItem>("Store", TextCatalog.StreamingAssets, true);

        public string ID {get; set;}
        public StorageElementFlags storageElementFlags { get; set; }
        
        [OnLaunch(0)]
        static void InitializeOnLoad() {
            if (OnceAccess.GetAccess("StoreItem"))
                storage.ForEach(i => i.Initialize());
        }

        protected virtual void Initialize() { }

        
        [LocalizationKeysProvider]
        static IEnumerable GetKeys() {
            return storage.items.CastIfPossible<ILocalized>();
        }
        
        #region ISerializable
        
        public virtual void Serialize(Writer writer) {
            writer.Write("ID", ID);
            writer.Write("storageElementFlags", storageElementFlags);
        }

        public virtual void Deserialize(Reader reader) {
            ID = reader.Read<string>("ID");
            storageElementFlags = reader.Read<StorageElementFlags>("storageElementFlags");
        }

        #endregion
    }
    
    public abstract class StoreGood : StoreItem, ILocalized, IBody, IVirtualizedScrollItem {
        
        public string iconName;
        public string group;
        public int order = 0;
        
        public virtual Type BodyType => typeof(StoreGoodButton);
        public string bodyName { get; set; }
        
        public bool alwaysAccess = false;
        
        public bool HasAccess() {
            return alwaysAccess || PlayerData.store.HasAccess(ID);
        }
        
        #region Localizion

        public const string localizationNameKeyFormat = "Store/{0}/title";
        public const string localizationDetailsKeyFormat = "Store/{0}/details";
        
        public const string lockedLocalizationKey = StoreList.localizationKeyPath + "locked";
        
        public string LocalizatedNameKey => localizationNameKeyFormat.FormatText(ID);
        public string LocalizatedDetailsKey => localizationDetailsKeyFormat.FormatText(ID);
        
        public string LocalizatedName => Localization.content[LocalizatedNameKey];
        public string LocalizatedDetails => Localization.content[LocalizatedDetailsKey];
        
        public virtual IEnumerable GetLocalizationKeys() {
            yield return lockedLocalizationKey; 
            yield return LocalizatedNameKey; 
            yield return LocalizatedDetailsKey;
        }
        
        #endregion

        public virtual string GetDetails() => LocalizatedDetails;
        
        protected void SetAction(StoreGoodButton button, string label, Action action = null) {
            if (action == null) {
                button.buy.NoAction();
                
                if (button.noAction) {
                    button.buy.gameObject.SetActive(false);
                    
                    button.noAction.gameObject.SetActive(true);
                    button.noAction.text = label;
                } else {
                    button.buy.gameObject.SetActive(true);
                    button.cost.text = label;
                }
            } else {
                button.buy.gameObject.SetActive(true);
                button.buy.SetAction(action);
                
                if (button.noAction) 
                    button.noAction.gameObject.SetActive(false);

                button.cost.text = label;
            }
        }
        
        protected virtual void SetupLockButton(StoreGoodButton button) {
            SetAction(button,Localization.content[lockedLocalizationKey], 
                () => ComposedPage.ByID("Popup").Show(new UnlockPackOfferPage(ID)));
        }
        
        #region IVirtualizedScrollItem
        
        public virtual void SetupBody(VirtualizedScrollItemBody body) {
            if (body is StoreGoodButton button) {
                if (button.title) 
                    button.title.text = LocalizatedName;
                
                if (button.details) 
                    button.details.text = GetDetails();

                if (button.icon)
                    if (iconName.IsNullOrEmpty()) {
                        button.icon.sprite = null;
                        button.icon.enabled = false;                    
                    } else {
                        button.icon.sprite = AssetManager.GetAsset<Sprite>(iconName);
                        button.icon.enabled = true;                    
                    }
            }
        }

        public string GetBodyPrefabName() => bodyName;

        #endregion
        
        #region ISerializable
        
        public override void Serialize(Writer writer) {
            base.Serialize(writer);
            writer.Write("group", group);
            writer.Write("bodyName", bodyName);
            writer.Write("icon", iconName);
            writer.Write("order", order);
            writer.Write("alwaysAccess", alwaysAccess);
        }

        public override void Deserialize(Reader reader) {
            base.Deserialize(reader);
            reader.Read("group", ref group);
            bodyName = reader.Read<string>("bodyName");
            reader.Read("icon", ref iconName);
            reader.Read("order", ref order);
            reader.Read("alwaysAccess", ref alwaysAccess);
        }

        #endregion
    }
    
}