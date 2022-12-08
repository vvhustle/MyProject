using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;
using Yurowm.Extensions;
using Yurowm.GUIHelpers;
using Yurowm.HierarchyLists;
using Yurowm.Utilities;

namespace Yurowm.DebugTools {
    public class DebugPanelView : EditorWindow {
        
        List<DebugPanel.Entry> collection = new List<DebugPanel.Entry>();
        LogList logList;
        
        [MenuItem("Yurowm/DebugTools/Debug Panel")]
        static DebugPanelView CreateView() {
            var window = GetWindow<DebugPanelView>();
            window.Show();
            window.OnEnable();
            return window;
        }
        
        void OnEnable() {
            titleContent.text = "Debug Panel";
            
            DebugPanel.AddListener(this);
            
            DebugPanel.onLog += OnLog;
            EditorApplication.update += OnUpdate;
        }

        void OnDisable() {
            DebugPanel.RemoveListener(this);
            
            DebugPanel.onLog -= OnLog;
            EditorApplication.update -= OnUpdate;
        }

        #region Repaint

        bool dirty = true;
        
        void OnLog() {
            dirty = true;
        }
        
        void OnUpdate() {
            if (dirty) {
                dirty = false;
                
                collection.Reuse(DebugPanel.GetEntries());
                
                if (logList == null) {
                    logList = new LogList(collection);
                    logList.drawItem = DrawEntry;
                    logList.onDoubleClick = OnDoubleClick;
                    logList.onSelectedItemChanged += OnSelectedItemChanged;
                }
                else
                    logList.Reload();
                
                Repaint();
            }
        }

        #endregion
        
        void OnGUI() {
            DrawToolbar();

            using (GUIHelper.Vertical.Start(GUILayout.ExpandHeight(true)))
                logList?.OnGUI();
            
            DrawSelected();
        }
        
        void DrawToolbar() {
            using (GUIHelper.Toolbar.Start()) {
                if (GUILayout.Button("Clear", EditorStyles.toolbarButton, GUILayout.Width(50))) {
                    DebugPanel.Clear();
                    collection.Clear();
                    logList.folders.Clear();
                    logList.Reload();
                }
                
                if (GUILayout.Button("Show all", EditorStyles.toolbarButton, GUILayout.Width(60)))
                    logList.ExpandAll();
                
                if (GUILayout.Button("Hide all", EditorStyles.toolbarButton, GUILayout.Width(60))) 
                    logList.CollapseAll();
            }
            
        }

        void DrawEntry(Rect rect, DebugPanel.Entry entry) {
            using (GUIHelper.Color.Start(DebugPanel.GroupToColor(entry.group))) {
                if (!entry.name.StartsWith("~")) {
                    var prefixRect = rect;
                    
                    if (EditorGUIUtility.labelWidth > 0)
                        prefixRect.width = EditorGUIUtility.labelWidth;
                    else
                        prefixRect.width = 200;
                    
                    GUI.Label(prefixRect, entry.name);
                        
                    rect.xMin = prefixRect.xMax;
                }
                
                DrawMessage(rect, entry.message);
            }
        }
        
        void OnDoubleClick(DebugPanel.Entry entry) {
            MessageDrawer.Get(entry.message.GetType())?.DoubleClick(entry.message);
        }
        
        void DrawMessage(Rect rect, DebugPanel.IMessage message) {
            if (message != null)
                MessageDrawer.Get(message.GetType())?.Draw(rect, message);
        }

        #region Selected

        DebugPanel.Entry selected = null;
        GUIHelper.Scroll scroll = new GUIHelper.Scroll();
        
        GUIHelper.VerticalSplit splitter = new GUIHelper.VerticalSplit();
        
        void OnSelectedItemChanged(List<DebugPanel.Entry> list) { 
            var first = list.FirstOrDefault();
            selected = first;
        }

        void DrawSelected() {

            using (splitter.Start())
            using (scroll.Start()) {
                var message = selected?.message;
                
                if (message != null)
                    MessageDrawer.Get(message.GetType())?.DrawFull(message);
            }
        }
        
        #endregion
        
        class LogList : HierarchyList<DebugPanel.Entry> {
            public Action<Rect, DebugPanel.Entry> drawItem;

            public LogList(List<DebugPanel.Entry> collection) : base(collection, new List<TreeFolder>(), new TreeViewState()) {
                showAlternatingRowBackgrounds = true;
            }

            public override string GetPath(DebugPanel.Entry element) {
                return element.group;
            }

            public override void SetPath(DebugPanel.Entry element, string path) { }

            public override int GetUniqueID(DebugPanel.Entry element) {
                return element.hash;
            }

            public override DebugPanel.Entry CreateItem() {
                return null;
            }

            public override void ContextMenu(GenericMenu menu, List<IInfo> selected) {
                var entry = selected.FirstOrDefault()?.asItemKind?.content;
                
                if (entry == null) return;
                
                menu.AddItem(new GUIContent("Go to Declaration"), false, () =>
                    UnityEditorInternal.InternalEditorUtility
                        .OpenFileAtLineExternal(entry.logPoint.path, entry.logPoint.line, 0));
            }

            protected override bool CanStartDrag(CanStartDragArgs args) {
                return false;
            }

            protected override bool CanRename(TreeViewItem item) {
                return false;
            }

            public override void DrawItem(Rect rect, ItemInfo info) {
                drawItem?.Invoke(rect, info.content);
            }

            public override void DrawFolder(Rect rect, FolderInfo info) {
                using (GUIHelper.ContentColor.Start(DebugPanel.GroupToColor(info.name))) 
                    base.DrawFolder(rect, info);
            }
        }
    }
}