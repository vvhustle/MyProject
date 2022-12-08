using Yurowm.ObjectEditors;
using UnityEditor;

namespace Yurowm.Store {
    public class StorePackEditor : ObjectEditor<StorePack> {
        public override void OnGUI(StorePack pack, object context = null) {
            pack.offerOrder = EditorGUILayout.IntField("Offer Order", pack.offerOrder).ClampMin(0);
            EditList("Items", pack.items);
            EditStringList("Unlocks", pack.unlocks);
        }
    }
}