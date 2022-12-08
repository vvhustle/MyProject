using System.Linq;
using UnityEditor;
using UnityEngine;
using YMatchThree.Core;
using Yurowm.Extensions;
using Yurowm.GUIHelpers;
using Yurowm.GUIStyles;
using Yurowm.Nodes.Editor;

namespace YMatchThree.Editor {
    public class EmitBombsNodeEditor : NodeEditor<EmitBombsNode> {
        public override void OnNodeGUI(EmitBombsNode node, NodeSystemEditor editor = null) {
            GUILayout.Label($"{node.count} Bomb(s)");
            GUILayout.Label(node.bombIDs.Join("\n"));
        }

        public override void OnParametersGUI(EmitBombsNode node, NodeSystemEditor editor = null) {
            node.count = Mathf.Max(1, EditorGUILayout.IntField("Count", node.count));
            node.slotTag = EditorGUILayout.TextField("SlotTag", node.slotTag);

            GUILayout.Label("Bombs", Styles.title);
            foreach (var ID in node.bombIDs) {
                using (GUIHelper.Horizontal.Start()) {
                    if (GUILayout.Button("X", GUILayout.Width(20))) {
                        node.bombIDs.Remove(ID);
                        break;
                    }
                    GUILayout.Label(ID);
                }
            }
            if (GUILayout.Button("+", GUILayout.Width(20))) {
                GenericMenu menu = new GenericMenu();

                foreach (var bomb in LevelContent.storage.Items<Chip>().Where(c => c is BombChipBase)) {
                    if (node.bombIDs.Contains(bomb.ID)) continue;
                    
                    string id = bomb.ID;
                    menu.AddItem(new GUIContent(id), false, () => node.bombIDs.Add(id));
                }
                
                if (menu.GetItemCount() > 0)
                    menu.ShowAsContext();
            }
        }
    }
}