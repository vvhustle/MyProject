using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using YMatchThree.Core;
using Yurowm.Dashboard;
using Yurowm.GUIHelpers;
using Yurowm.Help;
using Yurowm.Icons;
using Yurowm.Nodes.Editor;

namespace YMatchThree.Editor {
    public class BuildLevelNodeEditor : NodeEditor<BuildLevelNode> {
        
        LevelLayoutPreviewRenderer previewRenderer = new LevelLayoutPreviewRenderer();

        public override void OnNodeGUI(BuildLevelNode node, NodeSystemEditor editor = null) {
            if (node.level == null)
                node.level = new Level();
            
            using (previewRenderer.Select(node.level)) {
                using (GUIHelper.Horizontal.Start()) {
                    previewRenderer.DrawLayout();
                    using (GUIHelper.Vertical.Start()) {
                        previewRenderer.DrawReport();
                        DrawEditButton(node, editor);
                    }
                }
            }

        }

        public override void OnParametersGUI(BuildLevelNode node, NodeSystemEditor editor = null) {
            DrawEditButton(node, editor);
        }

        void DrawEditButton(BuildLevelNode node, NodeSystemEditor editor) {
            if (GUILayout.Button("Edit")) {
                if (node.level == null)
                    node.level = new Level();
                
                if (editor?.window is YDashboard dashboard) {
                    var popup = EditorWindow.GetWindow<LevelLayoutEditorWindow>();
                    popup.Setup(node.level, editor);
                    dashboard.Show();
                }
            }
            EditorTips.PopLastRectByID("le.nodes.buildlevel.edit");
            
        }
    }

    public class LevelLayoutEditorWindow : EditorWindow {
        
        LevelLayoutEditor layoutEditor;
        
        public void Setup(Level nodeLevel, NodeSystemEditor editor) {
            if (layoutEditor == null)
                layoutEditor = new LevelLayoutEditor();
            
            layoutEditor.SetLevel(nodeLevel);
            layoutEditor.script = editor.nodeSystem as LevelScriptBase;
            layoutEditor.onGetDirty = editor.Save;
            
            layoutEditor.repaint = Repaint;   
            
            titleContent = new GUIContent("Level Layout");
        }
        public void OnGUI() {
            EditorTips.Start();
            
            if (layoutEditor?.design != null)
                layoutEditor.OnGUI();
            
            EditorTips.Draw(position);
        }
    }
}