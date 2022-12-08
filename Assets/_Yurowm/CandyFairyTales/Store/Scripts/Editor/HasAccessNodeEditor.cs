using UnityEditor;
using Yurowm.Nodes.Editor;

namespace Yurowm.Store {
    public class HasAccessNodeEditor : NodeEditor<HasAccessNode> {
        public override void OnNodeGUI(HasAccessNode node, NodeSystemEditor editor = null) {
            OnParametersGUI(node, editor);
        }

        public override void OnParametersGUI(HasAccessNode node, NodeSystemEditor editor = null) {
            node.accessKey = EditorGUILayout.TextField("Key", node.accessKey);
        }
    }
}