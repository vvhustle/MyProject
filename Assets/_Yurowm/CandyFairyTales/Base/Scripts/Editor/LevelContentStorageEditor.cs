using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Yurowm.Dashboard;
using Yurowm.Extensions;
using Yurowm.GUIHelpers;
using Yurowm.Serialization;

namespace YMatchThree.Core {
    [DashboardGroup("Content")]
    [DashboardTab("Level Content", null, "tab.levelcontent")]
    public class LevelContentStorageEditor : StorageEditor<LevelContent> {
        
        public override string GetItemName(LevelContent item) {
            return item.ID;
        }
        
        public override string GetFolderName(LevelContent item) {
            return item.GetContentBaseType().Name;
        }

        public override Storage<LevelContent> OpenStorage() {
            return LevelContent.storage;
        }
    }
}