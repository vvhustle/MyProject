using Yurowm.Dashboard;
using Yurowm.Serialization;

namespace Yurowm.Offers {
    [DashboardGroup("Content")]
    [DashboardTab("Offers", null, "tab.offers")]
    public class OfferStorageEditor : StorageEditor<Offer> {
        public override string GetItemName(Offer item) {
            return item.ID;
        }

        public override Storage<Offer> OpenStorage() {
            return Offer.storage;
        }
    }
}