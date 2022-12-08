using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using YMatchThree.Core;
using Yurowm.HierarchyLists;
using Yurowm.Nodes.Editor;
using Yurowm.ObjectEditors;

namespace YMatchThree.Editor {
    public class LayersControllerNodeEditor : NodeEditor<LayersControllerNode> {
        
        YList<LayersControllerNode.LayerAction> layersList = new YList<LayersControllerNode.LayerAction>();
        
        public LayersControllerNodeEditor() {
            layersList.getName = l => $"{l.layerID} ({l.state})";
            layersList.getNewOptions = new Dictionary<string, Func<LayersControllerNode.LayerAction>> {
                { "Layer", () => new LayersControllerNode.LayerAction {
                    layerID = "Layer"
                }}
            };
        }
        
        public override void OnNodeGUI(LayersControllerNode node, NodeSystemEditor editor = null) {
            foreach (var layer in node.layers)
                GUILayout.Label($"{layer.layerID} ({layer.state})");
        }

        public override void OnParametersGUI(LayersControllerNode node, NodeSystemEditor editor = null) {
            layersList.OnGUILayout("Layers", node.layers);
            
            var selectedLayer = layersList.GetSelected(node.layers).FirstOrDefault();
            if (selectedLayer != null)
                ObjectEditor.Edit(selectedLayer, editor);
        }
    }
    
    public class LayerActionEditor : ObjectEditor<LayersControllerNode.LayerAction> {
        public override void OnGUI(LayersControllerNode.LayerAction layer, object context = null) {
            layer.layerID = EditorGUILayout.TextField("Layer ID", layer.layerID);
            layer.state = (LayersControllerNode.LayerState) EditorGUILayout.EnumPopup("Layer State", layer.state);
        }
    }
}