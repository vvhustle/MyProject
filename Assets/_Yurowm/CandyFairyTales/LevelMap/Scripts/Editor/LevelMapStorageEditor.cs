using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using YMatchThree.Seasons;
using Yurowm.Dashboard;
using Yurowm.GUIHelpers;
using Yurowm.Serialization;

namespace YMatchThree.Editor {
    [DashboardGroup("Content")]
    [DashboardTab("Level Maps", null, "tab.levelmaps")]
    public class LevelMapStorageEditor : StorageEditor<LevelMap> {

        public override string GetItemName(LevelMap item) {
            return item.ID;
        }

        public override Storage<LevelMap> OpenStorage() {
            return LevelMap.storage;
        }

        protected override void Sort() {}
    }
}