using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YMatchThree.Core;
using YMatchThree.Seasons;
using Yurowm.Colors;
using Yurowm.ContentManager;
using Yurowm.UI;

namespace YMatchThree.Meta {
    public class LevelSeasonVirtualizedScrollItem : VirtualizedScrollItem<LevelMap> {
        public TMP_Text title;
        
        public Image preview;
        
        public override IList SourceProvider() {
            return LevelMap.storage.items;
        }

        UIColorScheme scheme = new UIColorScheme();
        
        public override void UpdateInfo(LevelMap info) {
            title.text = info.worldName;

            info.FillColorScheme(scheme);
            gameObject.Repaint(scheme);
        }
    }
}