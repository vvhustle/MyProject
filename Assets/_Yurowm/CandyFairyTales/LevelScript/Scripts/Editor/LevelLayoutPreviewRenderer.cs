using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using YMatchThree.Core;
using Yurowm.Extensions;
using Yurowm.GUIHelpers;
using Yurowm.GUIStyles;
using Yurowm.Icons;
using Yurowm.Utilities;

namespace YMatchThree.Editor {
    public class LevelLayoutPreviewRenderer : IDisposable {
        
        Dictionary<Level, Preview> previews = new Dictionary<Level, Preview>();
        
        DelayedAccess cleanAccess = new DelayedAccess(30);
        TimeSpan removePreviewDelay = TimeSpan.FromSeconds(30);
        
        Preview currentPreview;
        
        public LevelLayoutPreviewRenderer Select(Level level) {
            if (cleanAccess.GetAccess()) {
                var now = DateTime.Now;
                previews = previews.RemoveAll(p => 
                    p.Value.buildTime + removePreviewDelay < now);
            }
            
            if (!previews.TryGetValue(level, out currentPreview)) {
                currentPreview = new Preview(level);
                previews.Add(level, currentPreview);
            }
            
            currentPreview.OnSelect();
            
            return this;
        }
        
        public void DrawLayout() {
            currentPreview?.DrawLayout();
        }
        
        public void DrawReport() {
            currentPreview?.DrawReport();
        }
        
        public void Dispose() {
            currentPreview = null;
        }
        
        class Preview {
            enum IconType {
                Empty,
                Slot,
                Chip,
                Block
            } 
            
            Icon[,] icons;
            area area;
            Vector2 size;
            
            GUIContent report;
            
            Level level;
            
            const float slotSize = 7;
            const float slotOffset = 1;
            static readonly TimeSpan buildDelay = TimeSpan.FromSeconds(5);
            
            static readonly Color slotColor = new Color(0.2f, 0.2f, 0.2f);
            static GUIStyle reportStyle;
            static Texture2D iconTexture;
            
            public DateTime buildTime {
                private set;
                get;
            }

            public Preview(Level level) {
                this.level = level;
            }
            
            void Generate() {
                area = new area(level.slots.Select(s => s.coordinate));

                if (area.width > 0 && area.height > 0) {
                    icons = new Icon[level.width, level.height];

                    foreach (var coord in area.GetPoints()) {
                        var slot = level.slots.FirstOrDefault(s => s.coordinate == coord);
                        
                        Icon icon = new Icon();

                        if (slot == null)
                            icon.type = IconType.Empty;
                        else if (slot.HasBaseContent<Block>()) {
                            icon.type = IconType.Block;
                            if (ChipSlotDrawer.GetColor(slot.GetContent<Block>(), out var color))
                                icon.color = color;
                        } else if (slot.HasBaseContent<Chip>()) {
                            icon.type = IconType.Chip;
                            if (ChipSlotDrawer.GetColor(slot.GetContent<Chip>(), out var color))
                                icon.color = color;
                        } else
                            icon.type = IconType.Slot;

                        icons[coord.X, coord.Y] = icon;
                    }
                    
                    size = new Vector2(
                        area.width * slotSize + (area.width + 1) * slotOffset, 
                        area.height * slotSize + (area.height + 1) * slotOffset);
                } else {
                    icons = null;
                    size = default;
                }
                
                buildTime = DateTime.Now;
                
                var reportBuilder = new StringBuilder();
                
                reportBuilder.AppendLine(level.gamePlay.Bold());
                foreach (var group in level.extensions.GroupBy(c => c.ID))
                    reportBuilder.AppendLine($"{group.Key}{(group.Count() > 1 ? " x" + group.Count() : "")}");
                
                report = new GUIContent(reportBuilder.ToString().Trim());
            }

            public void OnSelect() {
                if (buildTime + buildDelay <= DateTime.Now) 
                    Generate();
            }
            
            public void DrawReport() {
                if (reportStyle == null) {
                    reportStyle = new GUIStyle(Styles.miniLabel);
                    reportStyle.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
                    reportStyle.alignment = TextAnchor.UpperLeft;
                    reportStyle.wordWrap = true;
                    reportStyle.richText = true;
                    reportStyle.fontSize = 10;
                }
                
                var size = reportStyle.CalcSize(report);
                
                var rect = EditorGUILayout.GetControlRect(GUILayout.ExpandWidth(true), GUILayout.MinHeight(size.y), GUILayout.ExpandHeight(true));  
                
                if (report != null) 
                    GUI.Label(rect, report, reportStyle);    
            }

            public void DrawLayout() {
                var rect = EditorGUILayout.GetControlRect(GUILayout.Width(size.x), GUILayout.Height(size.y));  
                
                if (Event.current.type != EventType.Repaint) 
                    return;

                if (icons.IsEmpty())
                    return;
                
                if (!iconTexture)
                    iconTexture = EditorIcons.GetIcon("ChipIcon");

                Handles.DrawSolidRectangleWithOutline(rect, Color.black, Color.clear);

                for (var x = area.left; x <= area.right; x++)
                for (var y = area.down; y <= area.up; y++) {
                    var slotRect = new Rect(
                        rect.x + x * slotSize + (x + 1) * slotOffset,
                        rect.yMax - ((y + 1) * slotSize + (y + 1) * slotOffset),
                        slotSize, slotSize);

                    var icon = icons[x, y];
                    
                    switch (icon.type) {
                        case IconType.Empty: break;
                        case IconType.Slot:
                            Handles.DrawSolidRectangleWithOutline(slotRect, slotColor, Color.clear);
                            break;
                        case IconType.Chip: {
                            Handles.DrawSolidRectangleWithOutline(slotRect, slotColor, Color.clear);
                            using (GUIHelper.Color.Start(icon.color))
                                GUI.DrawTexture(slotRect, iconTexture);
                            break;
                        }
                        case IconType.Block:
                            Handles.DrawSolidRectangleWithOutline(slotRect, icon.color, Color.clear);
                            break;
                    }
                }
            }

            struct Icon {
                public IconType type;
                public Color color;
            }   
        }
    }
}