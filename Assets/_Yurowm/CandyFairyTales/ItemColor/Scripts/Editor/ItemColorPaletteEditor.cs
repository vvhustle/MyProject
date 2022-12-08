using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using YMatchThree.Core;
using Yurowm.GUIHelpers;
using Yurowm.GUIStyles;
using Yurowm.Help;
using Yurowm.ObjectEditors;

namespace YMatchThree.Editor {
    public class ItemColorPaletteEditor : ObjectEditor<ItemColorPalette>  {
        public override void OnGUI(ItemColorPalette obj, object context = null) {
            using (GUIHelper.Horizontal.Start()) {
                EditorGUILayout.PrefixLabel("Chip Colors");

                using (GUIHelper.Vertical.Start()) {
                    for (int i = 0; i < obj.colors.Count; i++) {
                        using (GUIHelper.Horizontal.Start()) {
                            if (GUILayout.Button("X", GUILayout.Width(20))) {
                                obj.colors.RemoveAt(i);    
                                break;
                            }
                            EditorTips.PopLastRectByID("lc.palette.remove");
                            
                            obj.colors[i] = EditorGUILayout.ColorField(obj.colors[i]);
                            EditorTips.PopLastRectByID("lc.palette.color");
                        }
                    }
                    if (obj.colors.Count < ItemColorInfo.IDs.Length)
                        using (GUIHelper.Horizontal.Start()) {
                            if (GUILayout.Button("NEW", GUILayout.Width(50)))
                                obj.colors.Add(Color.white);    
                            EditorTips.PopLastRectByID("lc.palette.new");
                        }
                }
            }
        }
    }
    
    public static class ItemColorEditorPalette {
        
        public static readonly ItemColorPalette instance = new ItemColorPalette();
        
        static ItemColorEditorPalette() {
            instance.colors = new List<Color>() {
                new Color(r: 1f, g: 0.12f, b: 0.19f),
                new Color(r: 0.17f, g: 1f, b: 0.24f),
                new Color(0.24f, 0.8f, 1f),
                new Color(1f, 0.85f, 0.15f),
                new Color(0.77f, 0.07f, 1f),
                new Color(1f, 0.45f, 0.13f)
            };
        }
        
        public static Color Get(int colorID) {
            return instance.Get(colorID);
        }
    }
}