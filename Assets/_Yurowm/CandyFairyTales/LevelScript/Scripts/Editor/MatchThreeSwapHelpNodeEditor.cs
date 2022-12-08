using UnityEditor;
using UnityEngine;
using YMatchThree.Core;
using Yurowm.Nodes.Editor;
using Yurowm.Spaces;

namespace Yurowm.Editors {
    public class MatchThreeSwapHelpNodeEditor : NodeEditor<MatchThreeSwapHelpNode> {
        public override void OnNodeGUI(MatchThreeSwapHelpNode node, NodeSystemEditor editor = null) {
            GUILayout.Label($"Layer: {node.layerID}");
        }

        public override void OnParametersGUI(MatchThreeSwapHelpNode node, NodeSystemEditor editor = null) {
            BaseTypesEditor.SelectAsset<HelperHand>(node, nameof(node.handName), h => editor?.SetDirty());
            node.layerID = EditorGUILayout.TextField("Layer ID", node.layerID);
        }
    }
}