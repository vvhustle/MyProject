using Yurowm.Dashboard;
using Yurowm.Serialization;
using Yurowm.Extensions;

namespace Yurowm.Store {
    [DashboardGroup("Content")]
    [DashboardTab("Store", "Coin", "tab.store")]
    public class StoreEditor : StorageEditor<StoreItem> {

        public override string GetFolderName(StoreItem item) {
            if (item is StoreGood good) {
                if (good.group.IsNullOrEmpty())
                    return "No Group";
                else
                    return good.group;
            }
            return "Non Goods";
        }

        public override string GetItemName(StoreItem item) {
            if (item is StoreGood good)
                return $"{good.order}. {item.ID}";
            return item.ID;
            
        }

        public override Storage<StoreItem> OpenStorage() {
            return StoreItem.storage;
        }

        protected override void Sort() {
            storage.items.Sort(i => {
                if (i is StoreGood good) 
                    return good.order;
                return 0;
            });
        }
    }
}