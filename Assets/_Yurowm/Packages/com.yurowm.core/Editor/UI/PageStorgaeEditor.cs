using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Yurowm.Dashboard;
using Yurowm.Extensions;
using Yurowm.Serialization;

namespace Yurowm.UI {
    [DashboardGroup("UI")]
    [DashboardTab("Pages", "Window", "tab.pages")]
    public class PageStorgaeEditor : StorageEditor<Page> {
        public override string GetItemName(Page item) {
            return item.ID;
        }

        public override Storage<Page> OpenStorage() {
            return Page.storage;
        }
        
        public PageStorgaeEditor() {
            UpdatePanels();
        }
        
        static Dictionary<int, UIPanel> panels;

        public override void OnFocus() {
            base.OnFocus();
            UpdatePanels();
        }

        void UpdatePanels() {
            panels = Object
                .FindObjectsOfType<UIPanel>(true)
                .ToDictionaryKey(p => p.linkID);
        }
        
        public static UIPanel GetPanel(int instanceID) {
            return panels.Get(instanceID);
        }
        
        public static IEnumerable<UIPanel> GetPanels() {
            return panels.Values;
        }
    }
}