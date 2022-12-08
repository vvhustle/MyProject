using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using YMatchThree.Editor;
using Yurowm.Help;
using Yurowm.ObjectEditors;

namespace YMatchThree.Core {
    public class FixedSizeColorSettingsEditor : ObjectEditor<FixedSizeColorSettings> {
        public override void OnGUI(FixedSizeColorSettings settings, object context = null) {
            var palette = settings.colorPalette ?? ItemColorEditorPalette.instance;
            
            settings.count = EditorGUILayout.IntSlider("Count", settings.count, 2, 
                Mathf.Min(ItemColorInfo.IDs.Length, palette.colors.Count));
            EditorTips.PopLastRectByID("lc.fixedsizecolors.count");
        }
    }
}