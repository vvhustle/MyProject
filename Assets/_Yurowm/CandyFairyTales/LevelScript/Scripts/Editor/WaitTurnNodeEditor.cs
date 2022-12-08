using UnityEditor;
using UnityEngine;
using YMatchThree.Core;
using Yurowm;
using Yurowm.Nodes.Editor;

namespace YMatchThree.Editor {
    public class WaitTurnNodeEditor : NodeEditor<WaitTurnNode> {
        public override void OnNodeGUI(WaitTurnNode node, NodeSystemEditor editor = null) {
            GUILayout.Label($"Wait {node.turnsCount} turn(s)");
        }

        public override void OnParametersGUI(WaitTurnNode node, NodeSystemEditor editor = null) {
            node.turnsCount = EditorGUILayout.IntField("Turns Count", node.turnsCount).ClampMin(1);
        }
    }
}