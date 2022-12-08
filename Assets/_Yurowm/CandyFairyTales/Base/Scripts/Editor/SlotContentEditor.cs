using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using YMatchThree.Core;
using Yurowm;
using Yurowm.Help;
using Yurowm.ObjectEditors;
using Yurowm.Spaces;
using Yurowm.Utilities;

namespace YMatchThree.Editor {
    public class SlotContentEditor : ObjectEditor<SlotContent> {
        
        public class ChipEditor : ObjectEditor<Chip> {
            public override void OnGUI(Chip chip, object context = null) {
            }
        }
        
        public override void OnGUI(SlotContent content, object context = null) {
            content.shortName = EditorGUILayout.TextField("Short Name (2-3 chars)", content.shortName);
            EditorTips.PopLastRectByID("lc.chip.shortname");
            
            content.color = EditorGUILayout.ColorField("Color", content.color); 
            EditorTips.PopLastRectByID("lc.chip.color");
            
            BaseTypesEditor.SelectAsset<Sprite>(content, nameof(content.miniIcon));
            EditorTips.PopLastRectByID("lc.chip.miniicon");
            
            if (content is IDestroyable) {
                content.destroyingDelay = EditorGUILayout.FloatField("Destroying Delay", content.destroyingDelay).ClampMin(0);
                EditorTips.PopLastRectByID("lc.chip.destroyingdelay");
            }
        }
    }
}