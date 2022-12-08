using UnityEditor;
using UnityEngine;
using YMatchThree.Core;
using Yurowm.Extensions;
using Yurowm.Nodes.Editor;
using Yurowm.Serialization;

namespace YMatchThree.Editor {
    public class LevelCompleteNodeEditor : NodeEditor<LevelCompleteNode> {
        
        StorageSelector<LevelContent> bonusSelector = 
            new StorageSelector<LevelContent>(LevelContent.storage, 
                c => c?.ID, 
                c => c is CompleteBonus);

        
        public override void OnNodeGUI(LevelCompleteNode node, NodeSystemEditor editor = null) {
            EditorGUILayout.LabelField("Bonus", node.bonusID);
        }

        public override void OnParametersGUI(LevelCompleteNode node, NodeSystemEditor editor = null) {
            if (node.bonusID.IsNullOrEmpty())
                node.bonusID = LevelContent.storage.Items<CompleteBonus>().IDefaultOrFirst()?.ID;
            
            node.completeType = (LevelCompleteNode.CompleteType) EditorGUILayout.EnumPopup("Type", node.completeType);
            
            bonusSelector.Draw("Bonus", c => {
                node.bonusID = c.ID;
                editor.SetDirty();
            });  
        }
    }
}