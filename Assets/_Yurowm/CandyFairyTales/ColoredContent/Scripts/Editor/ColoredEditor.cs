using UnityEditor;
using UnityEngine;
using YMatchThree.Core;
using Yurowm;
using Yurowm.GUIHelpers;
using Yurowm.ObjectEditors;

namespace YMatchThree.Editor {
    public class ColoredEditor : ObjectEditor<IColored> {
        public override void OnGUI(IColored colored, object context = null) {
            
            
            // colored.colorInfo = DrawSingle(context is LevelEditorContext levelEditorContext ? levelEditorContext.design : null, colored.colorInfo);
        }
        
        public ItemColorInfo DrawSingle(LevelColorSettings colorSettings, ItemColorInfo colorInfo) {
            Rect rect = EditorGUILayout.GetControlRect(GUILayout.ExpandWidth(true), GUILayout.Height(EditorGUIUtility.singleLineHeight));
            
            if (Event.current.type == EventType.Layout) return colorInfo;

            Rect rect2 = EditorGUI.PrefixLabel(rect, new GUIContent("Color Group"));
            rect.xMin = rect2.x;

            int colorCount = ItemColorInfo.IDs.Length;
            if (colorSettings != null)
                colorCount = Mathf.Min(colorCount, colorSettings.Count);
            
            Rect buttonRect = new Rect(rect);
            buttonRect.width /= colorCount + 1;

            if (GUI.Toggle(buttonRect, colorInfo.type == ItemColor.KnownRandom, "X", EditorStyles.miniButtonLeft) != (colorInfo.type == ItemColor.KnownRandom))
                colorInfo.type = ItemColor.KnownRandom;

            buttonRect.x += buttonRect.width;

            for (int i = 0; i < colorCount; i++) {
                GUIStyle style;
                if (i == colorCount - 1) style = EditorStyles.miniButtonRight;
                else style = EditorStyles.miniButtonMid;

                using (GUIHelper.BackgroundColor.Start(ItemColorEditorPalette.Get(i))) {
                    var state = colorInfo.type == ItemColor.Known && colorInfo.ID == i;
                    if (GUI.Toggle(buttonRect, state, "", style) != state) {
                        colorInfo.ID = i;
                        colorInfo.type = ItemColor.Known;
                    }
                    buttonRect.x += buttonRect.width;
                }
            }

            return colorInfo;
        }
    }
}