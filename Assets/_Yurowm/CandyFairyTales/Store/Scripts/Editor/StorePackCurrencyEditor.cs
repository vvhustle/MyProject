using Yurowm.ObjectEditors;
using UnityEditor;
using YMatchThree.Core;
using Yurowm.Serialization;
using Yurowm.Store;

namespace Yurowm.Editors {
    public class StorePackCurrencyEditor : ObjectEditor<StorePackCurrency> {
        
        StorageSelector<StoreItem> currencySelector = 
            new StorageSelector<StoreItem>(StoreItem.storage, 
                c => c?.ID, 
                c => c is Currency);
        
        public override void OnGUI(StorePackCurrency pack, object context = null) {
            currencySelector.Select(c => c.ID == pack.currencyID);
            currencySelector.Draw("Currency", c => pack.currencyID = c?.ID);
            
            pack.currencyCount = EditorGUILayout.IntField("Count", pack.currencyCount).ClampMin(1);
        }
    }
}