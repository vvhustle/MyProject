using UnityEditor;
using UnityEngine;
using Yurowm.Dashboard;
using Yurowm.Extensions;
using Yurowm.GUIHelpers;
using Yurowm.GUIStyles;
using Yurowm.ObjectEditors;
using Yurowm.Serialization;

namespace Yurowm.Help {
    [DashboardGroup("Development")]
    [DashboardTab("Tips <i>Beta</i>", "TipsIcon", "tab.tips")]
    public class EditorTipsEditor : StorageEditor<TipEntry> {
        
        int newTag;
        int emptyTag;
        
        public override bool Initialize() {
            newTag = tags.New("New");
            emptyTag = tags.New("Empty");
            return base.Initialize();
        }

        public override string GetItemName(TipEntry item) {
            return item.ID;
        }

        protected override Rect DrawItem(Rect rect, TipEntry item) {
            tags.Set(item, emptyTag, item.text.IsNullOrEmpty());
            return base.DrawItem(rect, item);
        }

        public override Storage<TipEntry> OpenStorage() {
            return EditorTips.storage;
        }

        protected override void OnOtherContextMenu(GenericMenu menu) {
            menu.AddItem(new GUIContent("Add Missed Items"), false, () => {
                EditorTips.missedIDs.ForEach(id => {
                    var entry = EditorTips.storage.GetItemByID(id);
                    if (entry != null) return;
                    entry = new TipEntry {
                        ID = id
                    };
                    tags.Set(entry, newTag, true);
                    EditorTips.storage.items.Add(entry);
                });
                Reload();
            });
            
            menu.AddItem(new GUIContent("Debug Mode"), EditorTips.debugMode, () => 
                EditorTips.debugMode = !EditorTips.debugMode);
            
            base.OnOtherContextMenu(menu);
        }
    }
    
    public class TipEntryEditor : ObjectEditor<TipEntry> {
        public override void OnGUI(TipEntry tip, object context = null) {
            using (GUIHelper.Horizontal.Start()) {
                EditorGUILayout.PrefixLabel("Text");
                tip.text = EditorGUILayout.TextArea(tip.text, Styles.textAreaLineBreaked); 
            }
        }
    }
}