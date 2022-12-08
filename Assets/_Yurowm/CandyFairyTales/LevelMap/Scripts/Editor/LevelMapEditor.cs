using UnityEditor;
using UnityEngine;
using YMatchThree.Core;
using YMatchThree.Seasons;
using Yurowm.ContentManager;
using Yurowm.ObjectEditors;
using Yurowm.Shapes;
using Yurowm.Spaces;

namespace YMatchThree.Editor {
    public class LevelMapEditor : ObjectEditor<LevelMap> {
        
        public override void OnGUI(LevelMap map, object context = null) {
            map.worldName = EditorGUILayout.TextField("World", map.worldName);

            BaseTypesEditor.SelectAsset<LevelButtonBody>("Level Button", map, nameof(map.levelButtonName));
            BaseTypesEditor.SelectAsset<YLine2D>("Level Line", map, nameof(map.levelLineName));
            
            map.fieldColor = EditorGUILayout.ColorField("Field Color", map.fieldColor);
            
            void AddNewLocation() {
                var menu = new GenericMenu();
                
                AssetManager.Instance.Initialize();

                foreach (var location in AssetManager.GetPrefabList<LevelMapLocationBody>()) {
                    var _l = location;
                    menu.AddItem(new GUIContent(location.name), false, 
                        () => map.locations.Add(_l.name));
                }
                        
                if (menu.GetItemCount() > 0)
                    menu.ShowAsContext();
            }
            
            
            EditList("Locations", map.locations,
                l => EditorGUILayout.LabelField(l),
                AddNewLocation);
        }
    }
}