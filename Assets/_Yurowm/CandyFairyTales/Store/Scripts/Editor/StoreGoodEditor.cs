using UnityEditor;
using UnityEngine;
using Yurowm.Localizations;
using Yurowm.ObjectEditors;
using Yurowm.Spaces;

namespace Yurowm.Store {

    public class StoreGoodEditor : ObjectEditor<StoreGood> {
        public override void OnGUI(StoreGood item, object context = null) {
            item.group = EditorGUILayout.TextField("Group", item.group);
            item.order = EditorGUILayout.IntField("Order", item.order);
        
            item.alwaysAccess = EditorGUILayout.Toggle("Always Access", item.alwaysAccess);
        
            BaseTypesEditor.SelectAsset<Sprite>(item, nameof(item.iconName));
            
            LocalizationEditor.EditOutside("Name", item.LocalizatedNameKey);
            
            LocalizationEditor.EditOutside("Details", item.LocalizatedDetailsKey);
        }
    }
}