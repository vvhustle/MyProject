using Yurowm.ObjectEditors;
using UnityEditor;
using Yurowm.Serialization;

namespace Yurowm.Store {
    public class StorePackOfferEditor : ObjectEditor<StorePackOffer> {
        
        StorageSelector<StoreItem> packSelector = 
            new StorageSelector<StoreItem>(StoreItem.storage, 
                c => c?.ID, 
                c => c == null || c is StorePack);

        public override void OnGUI(StorePackOffer offer, object context = null) {
            packSelector.Select(i => i.ID == offer.packID);
            packSelector.Draw("Pack", i => offer.packID = i?.ID);
        }
    }
}