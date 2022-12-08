using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using YMatchThree.Core;
using Yurowm.GUIHelpers;
using Yurowm.ObjectEditors;
using Yurowm.Serialization;

namespace Yurowm.Store {
    public class StorePackItemCountEditor : ObjectEditor<StorePackItemCount> {
        
        public override void OnGUI(StorePackItemCount item, object context = null) {
            item.count = EditorGUILayout.IntField("Count", item.count).ClampMin(1);
            
            using (GUIHelper.Horizontal.Start()) {
                item.itemID = EditorGUILayout.TextField("Item", item.itemID);
                if (GUILayout.Button("<", GUILayout.Width(20))) {
                    var menu = new GenericMenu();

                    foreach (var i in StoreItem.storage.Items<InventoryItem>()) {
                        var ID = i.ID;
                        menu.AddItem(new GUIContent(ID), item.itemID == ID, () => 
                            item.itemID = ID);
                    }
                    
                    if (menu.GetItemCount() > 0)
                        menu.ShowAsContext();
                }                
            }
        }
    }
}