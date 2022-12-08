using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using YMatchThree.Core;
using Yurowm.GUIHelpers;
using Yurowm.ObjectEditors;

namespace YMatchThree.Editor {
    public class PresetColorSettingsEditor : ObjectEditor<PresetColorSettings> {
        public override void OnGUI(PresetColorSettings settings, object context = null) {
            Rect rect = EditorGUILayout.GetControlRect(GUILayout.ExpandWidth(true), GUILayout.Height(EditorGUIUtility.singleLineHeight));

            if (Event.current.type == EventType.Layout) return;
            
            Rect rect2 = EditorGUI.PrefixLabel(rect, new GUIContent("Mask"));
            rect.xMin = rect2.x;

            var palette = settings.colorPalette ?? ItemColorEditorPalette.instance;
            
            Rect buttonRect = new Rect(rect);
            buttonRect.width /= palette.colors.Count;
            
            int count = Mathf.Min(ItemColorInfo.IDs.Length, palette.colors.Count);

            int editorColorID = 0;
            
            for (int id = 0; id < count; id++) {
                
                var used = settings.colorIDs.Contains(id);
                
                if (GUI.Toggle(buttonRect, used, "", EditorStyles.miniButtonMid) != used) {
                    used = !used;
                    
                    if (used) {
                        settings.colorIDs.Add(id);
                        settings.colorIDs.Sort();
                    } else {
                        if (settings.colorIDs.Count > 2)
                            settings.colorIDs.Remove(id);  
                    }
                }
                
                Handles.DrawSolidRectangleWithOutline(buttonRect,
                    palette.colors[id],
                    Color.black);
                
                if (used) {
                    Handles.DrawSolidRectangleWithOutline(
                        new Rect(buttonRect.position, Vector2.one * 6),
                        ItemColorEditorPalette.Get(editorColorID),
                        Color.black);
                    editorColorID++;
                }
                
                buttonRect.x += buttonRect.width;
            }
        }
    }
}