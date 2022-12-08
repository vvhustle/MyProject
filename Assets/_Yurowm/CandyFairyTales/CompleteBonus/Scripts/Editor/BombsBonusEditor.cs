using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using YMatchThree.Core;
using Yurowm;
using Yurowm.GUIHelpers;
using Yurowm.GUIStyles;
using Yurowm.Help;
using Yurowm.ObjectEditors;

namespace YMatchThree.Editor {
    public class BombsBonusEditor : ObjectEditor<BombsBonus> {
        public override void OnGUI(BombsBonus bonus, object context = null) {
            bonus.count = EditorGUILayout.IntField("Count", bonus.count).ClampMin(1);
            EditorTips.PopLastRectByID("lc.bombbonus.count");
            
            Edit("Explosion", bonus.explosion);
            EditorTips.PopLastRectByID("lc.bombbonus.explosion");
            
            EditorGUILayout.PrefixLabel("Bombs");
            EditorTips.PopLastRectByID("lc.bombbonus.bombs");

            using (GUIHelper.IndentLevel.Start()) {
                foreach (var ID in bonus.bombIDs) {
                    using (GUIHelper.Horizontal.Start()) {
                        if (GUILayout.Button("X", GUILayout.Width(20))) {
                            bonus.bombIDs.Remove(ID);
                            break;
                        }
                        EditorTips.PopLastRectByID("lc.bombbonus.remove");

                        GUILayout.Label(ID);
                        EditorTips.PopLastRectByID("lc.bombbonus.id");
                    }
                }

                if (GUILayout.Button("+", GUILayout.Width(20))) {
                    var menu = new GenericMenu();

                    foreach (var bomb in LevelContent.storage
                        .Items<Chip>()
                        .Where(c => c is BombChipBase)) {

                        if (bonus.bombIDs.Contains(bomb.ID)) continue;

                        string id = bomb.ID;
                        menu.AddItem(new GUIContent(id), false, () => bonus.bombIDs.Add(id));
                    }

                    if (menu.GetItemCount() > 0)
                        menu.ShowAsContext();
                }
                EditorTips.PopLastRectByID("lc.bombbonus.new");
            }
        }
    }
}