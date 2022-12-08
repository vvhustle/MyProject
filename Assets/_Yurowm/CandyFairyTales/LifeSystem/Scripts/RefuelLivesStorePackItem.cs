using Yurowm.Integrations;
using Yurowm.Store;
using Yurowm.UI;

namespace Yurowm.Features {
    public class RefuelLivesStorePackItem : StorePackItem {
        public override bool Has() {
            var lifeSystem = Integration.Get<LifeSystem>();
            if (lifeSystem == null) return true; 
            
            return lifeSystem.data.count >= lifeSystem.lifeCount;
        }

        public override void Redeem() {
            Integration.Get<LifeSystem>()?.SetFull();
            UIRefresh.Invoke();
        }
    }
}