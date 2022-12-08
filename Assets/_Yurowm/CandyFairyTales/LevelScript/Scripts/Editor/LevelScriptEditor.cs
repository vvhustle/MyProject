using UnityEditor;
using UnityEngine;
using YMatchThree.Core;
using Yurowm;
using Yurowm.Dashboard;
using Yurowm.Extensions;
using Yurowm.GUIHelpers;
using Yurowm.GUIStyles;
using Yurowm.Help;
using Yurowm.Nodes;
using Yurowm.Nodes.Editor;
using Yurowm.ObjectEditors;
using Yurowm.Utilities;

namespace YMatchThree.Editor {
    public class LevelScriptEditor : LevelEditorBase, IDefault {

        public bool isDefault { get; set; } = true;
        
        NodeSystemEditor nodeSystemEditor;

        GUIStyle titleStyle;
        GUIContent title = new GUIContent();
        
        static readonly Color titleBackgroundColor = new Color(0f, 0f, 0f, 0.4f);
        
        public override string GetName() {
            return "Script";
        }

        public override void OnSelectAnotherLevel(LevelScriptOrderedFile file) {
            nodeSystemEditor = null;
            
            if (file?.Script != null) {
                nodeSystemEditor = new NodeSystemEditor(file.Script, context.controller.window) {
                    onSave = context.SetDirty
                };
            }
        }

        public override void OnGUI() {
            if (nodeSystemEditor == null) 
                return;
            
            nodeSystemEditor.OnGUI(GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));

            #region Title

            {
                if (titleStyle == null) {
                    titleStyle = new GUIStyle(EditorStyles.whiteBoldLabel);
                    titleStyle.normal.textColor = Color.white;
                    titleStyle.fontSize = 10;
                    titleStyle.alignment = TextAnchor.LowerLeft;
                }
                
                var text = $"Level #{context.design.order}\nID: {context.design.ID}";
                
                if (!context.design.name.IsNullOrEmpty())
                    text += $"\nName: {context.design.name}";
                
                title.text = text;
                
                var size = titleStyle.CalcSize(title);
                
                var rect = nodeSystemEditor.rect;
                rect.width = rect.width.ClampMax(size.x);
                rect.height = rect.height.ClampMax(size.y);
                rect.y = nodeSystemEditor.rect.yMax - rect.height;
                
                GUIHelper.DrawRect(rect, titleBackgroundColor);
                
                GUI.Label(rect, title, titleStyle);
                EditorTips.PopRectByID(rect, "lle.title");
            }
            
            #endregion
        }

        public override void OnActionToolbarGUI() {
            base.OnActionToolbarGUI();
            
            if (GUILayout.Button("Parameters", EditorStyles.toolbarButton, GUILayout.Width(100))) {
                if (context.controller.window is YDashboard dashboard){
                    var popup = DashboardPopup.Create<ParamtersPopup>();
                    popup.script = context.design;
                    popup.editor = nodeSystemEditor;
                    dashboard.ShowPopup(popup);
                }
            }
            EditorTips.PopLastRectByID("lle.parameters");
        }
        
        public static string NewID() {
            return YRandom.main.GenerateKey(16);
        }
        
        class ParamtersPopup : DashboardPopup {
            public LevelScriptBase script;
            public NodeSystemEditor editor;

            public override void OnGUI() {
                using (GUIHelper.Change.Start(editor.SetDirty))
                    ObjectEditor.Edit(script, editor);
            }
        }
    }
}