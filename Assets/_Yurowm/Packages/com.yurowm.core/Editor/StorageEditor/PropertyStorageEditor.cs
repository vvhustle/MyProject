using UnityEditor;
using UnityEngine;
using Yurowm.Dashboard;
using Yurowm.GUIHelpers;
using Yurowm.GUIStyles;
using Yurowm.InUnityReporting;
using Yurowm.ObjectEditors;

namespace Yurowm.Serialization {
    public abstract class PropertyStorageEditor : DashboardEditor {
        protected IPropertyStorage storage { get; private set; }

        GUIHelper.LayoutSplitter splitter;
        GUIHelper.Scroll settingsScroll = new GUIHelper.Scroll(GUILayout.ExpandHeight(true));

        public override bool isScrollable => false;

        bool rawView = false;

        SerializableReportEditor rawViewDict = null;
        
        public override bool Initialize() {
            Load();

            rawViewDict = null;
            
            return true;
        }

        public override void OnGUI() {
            using (settingsScroll.Start()) {
                if (storage == null)
                    Load();
                
                GUILayout.Label($"<{storage.GetType().FullName}>", Styles.miniLabelBlack);
                
                if (rawView)
                    DrawRawItem(storage);
                else
                    ObjectEditor.Edit(storage, this);
                
                GUILayout.FlexibleSpace();
            }
        }
        
        void DrawRawItem(ISerializable item) {
            if (rawViewDict == null) {
                rawViewDict = new SerializableReportEditor();
                var report = new SerializableReport(item);
                report.Refresh();
                rawViewDict.SetProvider(report);
            }
            
            rawViewDict.OnGUI(GUILayout.Height(Mathf.Min(500, rawViewDict.TreeHeight)));
        }

        public virtual void OnStorageToolbarGUI() {}

        public override void OnToolbarGUI() {
            using (GUIHelper.Horizontal.Start(EditorStyles.toolbar, GUILayout.ExpandWidth(true))) {
                if (rawView != GUILayout.Toggle(rawView, "Raw", EditorStyles.toolbarButton, GUILayout.Width(50))) {
                    rawView = !rawView;
                    if (rawView) rawViewDict.Refresh();
                }

                OnStorageToolbarGUI();
                
                base.OnToolbarGUI();

                if (GUILayout.Button("Apply", EditorStyles.toolbarButton, GUILayout.Width(100)))
                    Save();
                if (GUILayout.Button("Reset", EditorStyles.toolbarButton, GUILayout.Width(100))) 
                    Load();
            }
        }

        protected abstract IPropertyStorage EmitNew();
        
        void Save() {
            PropertyStorage.Save(storage);
        }
        
        void Load() {
            if (storage == null)
                storage = EmitNew();
            PropertyStorage.Load(storage);
        }

    }
}