using System.Linq;
using Yurowm.ObjectEditors;
using UnityEditor;
using UnityEngine;
using YMatchThree.Core;
using Yurowm.GUIHelpers;
using Yurowm.Help;

namespace Yurowm.Editors {
    public class BombsBoosterEditor : ObjectEditor<BombsBooster> {
        public override void OnGUI(BombsBooster booster, object context = null) {
            
            Edit("Spawn Explosion", booster.spawnExplosion);
            EditorTips.PopLastRectByID("lc.bombsbooster.explosion");
            
            EditorGUILayout.PrefixLabel("Bombs");
            EditorTips.PopLastRectByID("lc.bombsbooster.bombs");

            using (GUIHelper.IndentLevel.Start()) {
                foreach (var ID in booster.bombIDs) {
                    using (GUIHelper.Horizontal.Start()) {
                        if (GUILayout.Button("X", GUILayout.Width(20))) {
                            booster.bombIDs.Remove(ID);
                            break;
                        }
                        EditorTips.PopLastRectByID("lc.bombsbooster.remove");

                        GUILayout.Label(ID);
                        EditorTips.PopLastRectByID("lc.bombsbooster.id");
                    }
                }

                if (GUILayout.Button("+", GUILayout.Width(20))) {
                    var menu = new GenericMenu();

                    foreach (var bomb in LevelContent.storage
                        .Items<Chip>()
                        .Where(c => c is BombChipBase)) {

                        string id = bomb.ID;
                        menu.AddItem(new GUIContent(id), false, () => booster.bombIDs.Add(id));
                    }

                    if (menu.GetItemCount() > 0)
                        menu.ShowAsContext();
                }
                
                EditorTips.PopLastRectByID("lc.bombsbooster.new");
            }
        }
    }
}