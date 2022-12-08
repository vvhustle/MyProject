using System.Linq;
using Yurowm.ObjectEditors;
using UnityEditor;
using UnityEngine;
using YMatchThree.Editor;

namespace YMatchThree.Core {
    public class FieldBoosterSetSelectedEditor : ContentSelectionEditor<FieldBoosterSet> {
        public override void OnGUI(ContentInfo[] selection, LevelFieldEditor fieldEditor) {
            var variable = selection.FirstOrDefault()?.GetVariable<BoosterSetVariable>();
            
            if (variable != null) {
                
                void AddNewBooster() {
                    var menu = new GenericMenu();

                    foreach (var booster in LevelContent.storage.Items<FieldBooster>()) {
                        var ID = booster.ID;
                        if (!variable.IDs.Contains(ID))
                            menu.AddItem(new GUIContent($"Add/{ID}"), false, () =>
                                variable.IDs.Add(ID));
                    }
                        
                    if (menu.GetItemCount() > 0)
                        menu.ShowAsContext();
                }
                
                EditList("Boosters", variable.IDs, id => {
                    EditorGUILayout.LabelField(id);
                    return id;
                }, AddNewBooster);
            }
        }
    }
}