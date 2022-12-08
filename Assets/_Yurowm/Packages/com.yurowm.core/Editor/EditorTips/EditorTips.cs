using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Yurowm.Extensions;
using Yurowm.GUIHelpers;
using Yurowm.GUIStyles;
using Yurowm.Icons;
using Yurowm.Serialization;
using Yurowm.Utilities;

namespace Yurowm.Help {
    public static class EditorTips {
        
        public static readonly Storage<TipEntry> storage = new Storage<TipEntry>("EditorTips", TextCatalog.EditorDefaultResources);
        
        static bool started = false;
        static Dictionary<Rect, TipInfo> tips = new Dictionary<Rect, TipInfo>();
        public static List<string> missedIDs = new List<string>();
        
        public static bool debugMode {
            get => EditorStorage.Instance.GetBool("tips.debug");
            set => EditorStorage.Instance.SetBool("tips.debug", value);
        }
        
        public static void Start() {
            if (!enabled)
                return;
            
            if (Event.current.type == EventType.Repaint) {
                started = true;
                tips.Clear();
            } else
                started = false;
        }
        
        static bool IsTipMode() => started && GUI.enabled;
        
        static void PopRect(Rect rect, TipInfo tipInfo) {
            if (!IsTipMode()) return;
            
            rect = GUIUtility.GUIToScreenRect(rect);
            
            tips[rect] = tipInfo;
        }
        
        public static void PopRect(Rect rect, string tip) {
            if (!IsTipMode()) return;
            
            PopRect(rect, new TipInfo {
                type = TipInfo.Type.Text,
                value = tip
            });
        }

        public static void PopRectByID(Rect rect, string ID) {
            if (!IsTipMode() && !ID.IsNullOrEmpty()) return;

            PopRect(rect, new TipInfo {
                type = TipInfo.Type.ID,
                value = ID
            });
        }
        
        public static void PopLastRect(string tip) {
            if (!IsTipMode()) return;
            
            PopRect(GUILayoutUtility.GetLastRect(), new TipInfo {
                type = TipInfo.Type.Text,
                value = tip
            });
        }
        
        public static void PopLastRectByID(string ID) {
            if (!IsTipMode() && !ID.IsNullOrEmpty()) return;
            
            PopRect(GUILayoutUtility.GetLastRect(), new TipInfo {
                type = TipInfo.Type.ID,
                value = ID
            });
        }

        #region Styles and Colors

        static GUIStyle tipStyle = null;

        static readonly Color defaultColorLite = new Color(0.5f, 0.4f, 0f);
        static readonly Color errorColorLite = Color.red;
        static readonly Color backgroundColorLite = new Color(1f, 1f, 1f, .6f);
        
        static readonly Color defaultColorPro = Color.yellow;
        static readonly Color errorColorPro = Color.red;
        static readonly Color backgroundColorPro = new Color(0f, 0f, 0f, .6f);
        
        static Color defaultColor => EditorGUIUtility.isProSkin ? defaultColorPro : defaultColorLite;
        static Color errorColor => EditorGUIUtility.isProSkin ? errorColorPro : errorColorLite;
        static Color backgroundColor => EditorGUIUtility.isProSkin ? backgroundColorPro : backgroundColorLite;
        
        #endregion

        public static void Draw(Rect windowRect) {
            if (!enabled)
                return;
            
            if (Event.current.type != EventType.Repaint)
                return;

            if (!started) 
                return;

            started = false;

            if (tips.Count <= 0) return;

            Rect guiRect = default;
            var mousePosition = new Rect(Event.current.mousePosition, Vector2.zero);
            mousePosition = GUIUtility.GUIToScreenRect(mousePosition);
                
            if (!windowRect.Contains(mousePosition.position)) return;

            {
                var distance = float.MaxValue;
                
                if (debugMode)
                    guiRect = tips.Keys.FirstOrDefault(r => r.Contains(mousePosition.center));
                else
                    guiRect = tips
                        .FirstOrDefault(p => {
                            if (!HasText(p.Value))
                                return false;
                            return p.Key.Contains(mousePosition.center);
                        }).Key;
                
                if (guiRect == default)
                    foreach (var pair in tips) {
                        if (!debugMode && !HasText(pair.Value))
                            continue;
                        
                        var d = RectDistance(pair.Key, mousePosition.center);
                        
                        if (d > distance) continue;
                        
                        distance = d;
                        guiRect = pair.Key;
                        
                        if (distance <= 0) break;
                    }
            }

            var tipInfo = tips.Get(guiRect);
            
            string text = null;
            
            var color = defaultColor; 
            
            switch (tipInfo.type) {
                case TipInfo.Type.Text: text = tipInfo.value; break;
                case TipInfo.Type.ID: {
                    var entry = storage.GetItemByID(tipInfo.value); 
                    if (entry == null || entry.text.IsNullOrEmpty()) {
                        if (!missedIDs.Contains(tipInfo.value)) {
                            missedIDs.Add(tipInfo.value);
                            if (!debugMode) {
                                Draw(windowRect);
                                EditorWindow.focusedWindow.Repaint();
                                return;
                            }
                        }
                        color = errorColor;
                        text = tipInfo.value;
                    } else {
                        if (debugMode)
                            text = $"{entry.ID}\n\n{entry.text}";
                        else
                            text = entry.text;
                    }
                    break;
                }
            }


            guiRect = GUIUtility.ScreenToGUIRect(guiRect);
            
            GUIHelper.DrawRectLine(guiRect, color, 5);

            var tipRect = new Rect(Event.current.mousePosition, new Vector2(200, 0));
            tipRect.y += 30;
            
            windowRect = GUIUtility.ScreenToGUIRect(windowRect);
            
            if (tipStyle == null) {
                tipStyle = new GUIStyle(Styles.multilineLabel);
                tipStyle.fontSize = (tipStyle.fontSize * .8f).RoundToInt();
                tipStyle.fontStyle = FontStyle.Bold;
                tipStyle.normal.textColor = Color.white;
                tipStyle.padding = new RectOffset(5, 3, 1, 1);
            }
            
            var content = new GUIContent(text);
            tipStyle.CalcMinMaxWidth(content, out float minWidth, out float maxWidth);
            tipRect.width = Mathf.Clamp(maxWidth, minWidth, 200) + 10;
            tipRect.height = tipStyle.CalcHeight(content, tipRect.width) + 10;
            
            tipRect.x -= tipRect.width / 2;
            
            tipRect.x = tipRect.x.Clamp(windowRect.xMin, windowRect.xMax - tipRect.width);
            tipRect.y = tipRect.y.Clamp(windowRect.yMin, windowRect.yMax - tipRect.height);
            
            Handles.DrawSolidRectangleWithOutline(tipRect, backgroundColor, Color.clear);

            using (GUIHelper.ContentColor.Start(color)) 
                GUI.Label(tipRect, content, tipStyle);

            GUIHelper.DrawRectLine(tipRect, color, 5);

            #region Bezier Line

            if (!guiRect.Overlaps(tipRect)) {
                Side startSide;

                if (guiRect.xMin > tipRect.xMax)
                    startSide = Side.Left;
                else if (guiRect.xMax < tipRect.xMin)
                    startSide = Side.Right;
                else if (guiRect.yMin > tipRect.yMax)
                    startSide = Side.Bottom;
                else
                    startSide = Side.Top;
                
                var endSide = startSide.Mirror();
                
                Vector2 SideToPoint(Rect rect, Side side) {
                    return new Vector2(
                        rect.center.x + rect.width * side.X() / 2,
                        rect.center.y + rect.height * side.Y() / 2);
                }
                
                Vector2 SideToTangent(Side side) {
                    return side.ToVector2();
                }
                
                var startPoint = SideToPoint(guiRect, startSide);
                var endPoint = SideToPoint(tipRect, endSide);
                
                var distance = (Vector2
                    .Distance(startPoint, endPoint) / 2)
                    .ClampMax(30);

                var startTan = startPoint + SideToTangent(startSide) * distance;
                var endTan = endPoint + SideToTangent(endSide) * distance;

                GUIHelper.DrawBezier(startPoint, endPoint,
                    startTan, endTan, 
                    color, color, 5);
            }

            #endregion
            
            EditorWindow.focusedWindow.Repaint();
        }
        
        static float RectDistance(Rect rect, Vector2 point) {
            return RangeDistance(rect.xMin, rect.xMax, point.x) + RangeDistance(rect.yMin, rect.yMax, point.y);
        }     
        
        static float RangeDistance(float min, float max, float point) {
            if (point < min) return min - point;
            
            if (point > max) return point - max;
            
            return 0;
        }

        static bool HasText(TipInfo i) {
            switch (i.type) {
                case TipInfo.Type.Text: return !i.value.IsNullOrEmpty();
                case TipInfo.Type.ID: return !missedIDs.Contains(i.value);
                default: return false;
            }
        }
        
        struct TipInfo {
            public enum Type {
                ID,
                Text
            }
            public Type type;
            public string value;
        }

        const string enabledKey = "editorTipsEnabled";
        static bool enabled {
            get => EditorStorage.Instance.GetBool(enabledKey);
            set => EditorStorage.Instance.SetBool(enabledKey, value);
        }
            
        static Texture2D tipsIcon;
        
        public static void Toggle() {
            if (!tipsIcon)
                tipsIcon = EditorIcons.GetIcon("TipsIcon");

            using (enabled ? GUIHelper.ContentColor.Start(defaultColor) : null) 
                enabled = GUILayout.Toggle(enabled, tipsIcon, EditorStyles.toolbarButton, GUILayout.Width(25));
        }
    }
    
    public class TipEntry : ISerializableID {
        public string text = "";
        public string ID { get; set; }
        
        public TipEntry() {}
        
        public TipEntry(string ID, string text) : this() {
            this.text = text;
            this.ID = ID;
        }

        public void Serialize(Writer writer) {
            writer.Write("ID", ID);
            writer.Write("text", text);
        }

        public void Deserialize(Reader reader) {
            ID = reader.Read<string>("ID");
            reader.Read("text", ref text);
        }

    }
}