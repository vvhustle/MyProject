using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using YMatchThree.Core;
using Yurowm.Help;
using Yurowm.ObjectEditors;
using Yurowm.Serialization;

namespace YMatchThree.Editor {
    public class LevelColorSettingsEditor : ObjectEditor<LevelColorSettings> {
        
        StorageSelector<LevelContent> colorSetSelector = 
            new StorageSelector<LevelContent>(LevelContent.storage, 
                c => c?.ID, 
                c => c is ItemColorPalette || c == null);
        
        public override void OnGUI(LevelColorSettings settings, object context = null) {
            colorSetSelector.Select(c => c.ID == settings.colorPalette?.ID);
            colorSetSelector.Draw("Palette", c => {
                settings.colorPalette = (ItemColorPalette) c;
                if (context is LevelEditorContext levelEditorContext)
                    levelEditorContext.SetDirty();
            });
            EditorTips.PopLastRectByID("le.colorsettings.palette");
        }
    }
}