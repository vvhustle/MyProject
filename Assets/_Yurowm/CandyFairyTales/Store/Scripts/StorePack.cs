using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using YMatchThree.Meta;
using Yurowm.Analytics;
using Yurowm.Core;
using Yurowm.Extensions;
using Yurowm.Localizations;
using Yurowm.Serialization;
using Yurowm.UI;

namespace Yurowm.Store {
    public abstract class StorePack : StoreGood {
        public List<string> unlocks = new List<string>();
        public List<StorePackItem> items = new List<StorePackItem>();
        
        public float savings = 0;
        
        public int offerOrder;
        public override void SetupBody(VirtualizedScrollItemBody body) {
            base.SetupBody(body);

            if (body is StoreGoodButton button) {
                if (IsOwned())
                    SetAction(button, Localization.content[ownKey]);
                else
                    SetupBuyButton(button);
                
                if (button.savings) {
                    button.savings.SetActive(savings > 0);
                    if (savings > 0 && button.savings.SetupChildComponent(out TMP_Text label))
                        label.text = Localization.content[savingsKey].FormatText((savings * 100).RoundToInt());
                }
            }
        }
        
        protected abstract void SetupBuyButton(StoreGoodButton button);

        public void Redeem() {
            PlayerData.store.AddKeychain(ID, unlocks);

            foreach (var item in items) 
                item.Redeem();

            Analytic.Event("StorePack_Redeem", Segment.New("storeItemID", ID));

            PlayerData.SetDirty();
            
            UIRefresh.Invoke();
        }

        public abstract void OnClick();

        public virtual bool IsOwned() {
            var storeData = PlayerData.store;
            
            return (unlocks.IsEmpty() || unlocks.All(u => storeData.HasAccess(u))) && 
                   (items.IsEmpty() || items.All(i => i.Has()));
        }
        
        public virtual IEnumerable<string> GetAccessKeys() {
            return unlocks;
        }
        
        #region Localization
        
        const string ownKey = "Store/own";
        const string savingsKey = "Store/savings";

        public override IEnumerable GetLocalizationKeys() {
            yield return base.GetLocalizationKeys();
            yield return ownKey;
            yield return savingsKey;
        }

        #endregion
        
        #region ISerializable

        public override void Serialize(Writer writer) {
            base.Serialize(writer);
            writer.Write("offerOrder", offerOrder);
            writer.Write("unlocks", unlocks.ToArray());
            writer.Write("items", items.ToArray());
            writer.Write("savings", savings); 
        }

        public override void Deserialize(Reader reader) {
            base.Deserialize(reader);
            reader.Read("offerOrder", ref offerOrder);
            
            unlocks.Reuse(reader
                .ReadCollection<string>("unlocks")
                .Where(u => !u.IsNullOrEmpty()));
            
            items.Reuse(reader
                .ReadCollection<StorePackItem>("items"));
            
            reader.Read("savings", ref savings);
        }
        
        #endregion
    }

    public abstract class StorePackItem : ISerializable {
        
        public abstract bool Has();
        
        public abstract void Redeem();
        public virtual void Serialize(Writer writer) { }

        public virtual void Deserialize(Reader reader) { }
    }
    
    public class StorePackItemCount : StorePackItem {
        public int count = 100;
        public string itemID;

        public override bool Has() {
            return false;
        }

        public override void Redeem() {
            PlayerData.inventory.EarnItems(itemID, count);
            UIRefresh.Invoke();
            
            PlayerData.SetDirty();
        }

        public override void Serialize(Writer writer) {
            base.Serialize(writer);
            writer.Write("itemID", itemID);
            writer.Write("count", count);
        }

        public override void Deserialize(Reader reader) {
            base.Deserialize(reader);
            reader.Read("itemID", ref itemID);
            reader.Read("count", ref count);
        }
    }
}