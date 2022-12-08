using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Yurowm;
using Yurowm.Icons;

namespace YMatchThree.Editor {
    public static class LevelEditorStyles {
        #region Styles
        static GUIStyle _labelStyle;
        public static GUIStyle labelStyle {
            get {
                if (_labelStyle == null) {
                    _labelStyle = new GUIStyle(GUI.skin.label);
                    _labelStyle.wordWrap = true;

                    _labelStyle.alignment = TextAnchor.MiddleCenter;

                    _labelStyle.normal.textColor = Color.black;
                    _labelStyle.focused.textColor = _labelStyle.normal.textColor;
                    _labelStyle.active.textColor = _labelStyle.normal.textColor;

                    _labelStyle.fontSize = 8;
                    _labelStyle.margin = new RectOffset();
                    _labelStyle.padding = new RectOffset();
                }
                return _labelStyle;
            }
        }

        static GUIStyle _tagStyle;
        public static GUIStyle tagStyle {
            get {
                if (_tagStyle == null) {
                    _tagStyle = new GUIStyle();                
                    _tagStyle.border = new RectOffset(1, 1, 1, 1);
                    _tagStyle.normal.textColor = Color.white;
                    _tagStyle.fontStyle = FontStyle.Bold;
                    _tagStyle.alignment = TextAnchor.MiddleCenter;
                    _tagStyle.fontSize = 8;
                    _tagStyle.margin = new RectOffset();
                    _tagStyle.padding = new RectOffset();
                }
                return _tagStyle;
            }
        }

        static GUIStyle _richLabelStyle;
        public static GUIStyle richLabelStyle {
            get {
                if (_richLabelStyle == null) {
                    _richLabelStyle = new GUIStyle(EditorStyles.label);
                    _richLabelStyle.richText = true;
                }
                return _richLabelStyle;
            }
        }

        static GUIStyle _levelLayoutTitleStyle;
        public static GUIStyle levelLayoutTitleStyle {
            get {
                if (_levelLayoutTitleStyle == null) {
                    _levelLayoutTitleStyle = new GUIStyle(EditorStyles.whiteLargeLabel);
                    _levelLayoutTitleStyle.alignment = TextAnchor.MiddleCenter;
                }
                return _levelLayoutTitleStyle;
            }
        }
        #endregion
        
        #region Icons
        public static Texture2D slotIcon;

        public static Texture2D listLevelIcon;
        public static Texture2D worldIcon;

        static void BuildIcons() {
            listLevelIcon = EditorIcons.GetIcon("Script");
            worldIcon = EditorIcons.GetIcon("ContentTabIcon");
            
            var orig = EditorIcons.GetIcon("SlotIcon");   
            slotIcon = new Texture2D(orig.width, orig.height, orig.format, orig.mipmapCount, true);
            Graphics.CopyTexture(orig, slotIcon);
        }
        #endregion

        static LevelEditorStyles() {
            BuildIcons();
        }
    }
}
