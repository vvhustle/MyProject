using UnityEditor;
using UnityEngine;
using Yurowm.Nodes.Editor;
using Yurowm.ObjectEditors;
using Yurowm.Spaces;

namespace Yurowm.Dialogues {
    public class DialogueNodeEditor : NodeEditor<DialogueNode> {
        public override void OnNodeGUI(DialogueNode node, NodeSystemEditor editor = null) {
            foreach (var instruction in node.instructions) 
                GUILayout.Label(instruction.ToString());
        }

        public override void OnParametersGUI(DialogueNode node, NodeSystemEditor editor = null) {
            ObjectEditor.EditList("Instructions", node.instructions, editor);
        }
    }
}