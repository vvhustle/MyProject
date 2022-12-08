using System.Collections.Generic;
using System.Linq;

namespace Yurowm.Store {
    public class UnlockPackOfferPage : StoreItemOfferPage {
        string unlockKey;
        
        public UnlockPackOfferPage(string unlockKey, string layoutName = nameof(StoreItemComposed)) : base(layoutName) {
            this.unlockKey = unlockKey;
            closeOnBuy = true;
        }
        
        public override IEnumerable<StoreGood> GetGoods() {
            return StoreItem.storage.Items<StorePack>()
                .Where(p => p.GetAccessKeys().Contains(unlockKey))
                .OrderBy(p => p.offerOrder);
        }
    }
}