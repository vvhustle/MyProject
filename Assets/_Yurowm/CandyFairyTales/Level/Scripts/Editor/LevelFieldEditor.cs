using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using YMatchThree.Core;
using Yurowm.Extensions;
using Yurowm.GUIHelpers;
using Yurowm.GUIStyles;
using Yurowm.Icons;
using Yurowm.Utilities;

namespace YMatchThree.Editor {
    public class LevelFieldEditor {
        
        public object context;
        
        public int2 fieldSize;
        public Dictionary<int2, SlotInfo> slots;
        public List<ContentInfo> extensions;

        Rect fieldRect;
        Vector2 slotSize = new Vector2(40, 40);
        area visibleArea = new area();
        
        enum ClickType { None, Main, Secondary }
        
        #region Events
        public Action<int2> onMainClick;
        public Action<int2> onSecondaryClick;
        public Action onPostDrawing;
        public Action<SlotInfo, Rect> onSlotDraw;
        
        public Action repaint = delegate {};
        public Func<SlotInfo, bool> slotMask = null;
        
        #endregion
        
        Color emptySlotColor = Color.white;
        
        public bool TryCoordToSlotRect(int2 coord, out Rect rect) {
            rect = fieldRect;

            if (coord == int2.Null || !visibleArea.Contains(coord)) 
                return false;
                
            rect.x += (slotSize.x + slotOffset) * (coord.X - visibleArea.left);
            rect.y += fieldRect.height - (slotSize.y + slotOffset) * (coord.Y - visibleArea.down + 1);
            rect.width = slotSize.x;
            rect.height = slotSize.y;
            
            return true;
        }

        bool horizontalScrollVisible;
        bool verticalScrollVisible;
        int2 scrollPosition = int2.zero;

        Texture2D slotIcon = EditorIcons.GetIcon("SlotIcon");
        
        public void PrepareToDraw() {
            Rect rect = EditorGUILayout.GetControlRect(
                GUILayout.ExpandWidth(true),
                GUILayout.ExpandHeight(true));
            
            if (Event.current.type == EventType.Repaint)
                Handles.DrawSolidRectangleWithOutline(rect, Color.black.Transparent(.7f), Color.clear);
            
            if (Event.current.type == EventType.Repaint) {
                var maxWidth = Mathf.FloorToInt ((rect.width - legendSize.x) / slotSize.x) - 1;
                var maxHeight = Mathf.FloorToInt ((rect.height - legendSize.y) / (slotSize.y * .75f)) - 1;
                
                horizontalScrollVisible = maxWidth < fieldSize.X;
                verticalScrollVisible = maxHeight < fieldSize.Y;
                
                scrollPosition.X = horizontalScrollVisible ? Mathf.Clamp(scrollPosition.X, 0, fieldSize.X - maxWidth) : 0;
                scrollPosition.Y = verticalScrollVisible ? Mathf.Clamp(scrollPosition.Y, 0, fieldSize.Y - maxHeight) : 0;
                
                visibleArea = new area(scrollPosition, new int2(
                    Mathf.Min(maxWidth, fieldSize.X), 
                    Mathf.Min(maxHeight, fieldSize.Y)));
                
                fieldRect.width = slotSize.x * visibleArea.width + (visibleArea.width - 1) * slotOffset;
                fieldRect.height = slotSize.y * visibleArea.height + (visibleArea.height - 1) * slotOffset;
                fieldRect.center = rect.center;
                
                if (horizontalScrollVisible) fieldRect.y -= 20f / 2;
                if (verticalScrollVisible) fieldRect.x -= 20f / 2;
            }
            
            DrawScrollbars(rect);
            
            if (GUI.enabled && rect.Contains(Event.current.mousePosition)) repaint.Invoke();
        }
        
        void DrawScrollbars(Rect rect) {
            if (horizontalScrollVisible) {
                Rect barRect = new Rect(rect.xMin, rect.yMax - 20f, rect.width - 20f, 20f);
                float value = GUI.HorizontalScrollbar(barRect, scrollPosition.X, visibleArea.width, 0, fieldSize.X);
                scrollPosition.X = Mathf.RoundToInt(value);
                visibleArea.position = scrollPosition;
            }
            if (verticalScrollVisible) {
                Rect barRect = new Rect(rect.xMax - 20, rect.yMin, 20f, rect.height - 20f);
                float value = GUI.VerticalScrollbar(barRect, scrollPosition.Y, visibleArea.height, fieldSize.Y, 0);
                scrollPosition.Y = Mathf.RoundToInt(value);
                visibleArea.position = scrollPosition;
            }
        }
        
        public void DrawFieldViewCoordinates() {
            if (Event.current.type != EventType.Repaint) return;

            #region Horizontal
            Rect r = new Rect(fieldRect.xMin, fieldRect.yMax, 
                slotSize.x, legendSize.y);
            for (int x = visibleArea.left; x <= visibleArea.right; x++) {
                GUI.Box(r, x.ToString(), Styles.centeredMiniLabelWhite);
                r.x += slotSize.x + slotOffset;
            }
            #endregion

            #region Vertical
            r = new Rect(fieldRect.xMin - legendSize.x, fieldRect.yMax - slotSize.y,
                legendSize.x, slotSize.y);
            for (int y = visibleArea.down; y <= visibleArea.up; y++) {
                GUI.Box(r, y.ToString(), Styles.centeredMiniLabelWhite);
                r.y -= slotSize.y + slotOffset;
            }
            #endregion
        }
        
        public void DrawFieldView() {
            if (Event.current.type == EventType.Layout) return;

            foreach (int2 coord in visibleArea.GetPoints()) {
                switch (DrawSlotButton(coord)) {
                    case ClickType.Main: onMainClick?.Invoke(coord); break;
                    case ClickType.Secondary: onSecondaryClick?.Invoke(coord); break;
                    case ClickType.None: break;
                }
            }

            if (Event.current.type == EventType.Repaint) {
                SlotContentDrawer.PostDraw(this);
                onPostDrawing?.Invoke();
            }
        }
        
        ClickType DrawSlotButton(int2 coord) {
            ClickType result = ClickType.None;

            if (!TryCoordToSlotRect(coord, out var rect))
                return result;
            
            var slot = slots.Get(coord);

            if (Event.current.type == EventType.MouseDown 
                && rect.Contains(Event.current.mousePosition)) {
                
                GUI.FocusControl("");
                
                switch (Event.current.button) {
                    case 0: result = ClickType.Main; break;
                    case 1: result = ClickType.Secondary; break;
                }
            }

            if (Event.current.type == EventType.Repaint) {
                bool visible = slot != null && (slotMask == null || slotMask.Invoke(slot));
                
                using (GUIHelper.Color.Start(emptySlotColor.Transparent(visible ? .2f : 0.05f)))
                    GUI.DrawTexture(rect, slotIcon);

                if (visible) {
                    SlotContentDrawer.Draw(rect, this, slot);
                    onSlotDraw?.Invoke(slot, rect);
                }    
            }

            return result;
        }
        
        public enum ResizeMode {
            Add,
            Remove
        }
        
        public void DrawFieldViewResizerButtons(Action<OrientationLine, int, ResizeMode> callback) {
            Rect r;

            if (!Event.current.alt) return;
            
            #region Horizontal Remove
            if (fieldSize.X > Level.minSize) {
                r = new Rect(fieldRect.xMin + slotSize.x / 4, fieldRect.yMax, 
                    slotSize.x / 2, legendSize.y);
                for (int x = visibleArea.left; x <= visibleArea.right; x++) {
                    
                    EditorGUIUtility.AddCursorRect(r, MouseCursor.ArrowMinus);
                    
                    if (r.Contains(Event.current.mousePosition)) {
                        if (Event.current.type == EventType.MouseDown)
                            callback.Invoke(OrientationLine.Horizontal, x, ResizeMode.Remove);
                            
                        if (Event.current.type == EventType.Repaint) {
                            GUIHelper.DrawRectLine(r, Color.red, 4);
                            
                            DrawOutlinedShape(Enumerator
                                .For(visibleArea.down, visibleArea.up, 1)
                                .Select(y => new int2(x, y))
                                .ToArray(), Color.red.Transparent(.2f), Color.red);
                        }
                        return;
                    }
                    
                    r.x += slotSize.x + slotOffset;
                }
            }
            #endregion
            
            #region Horizontal Add
            if (fieldSize.X < Level.maxSize) {
                r = new Rect(fieldRect.xMin - slotSize.x / 4, fieldRect.yMax, 
                    slotSize.x / 2, legendSize.y);
                for (int x = visibleArea.left; x <= visibleArea.right + 1; x++) {
                    
                    EditorGUIUtility.AddCursorRect(r, MouseCursor.ArrowPlus);
                    
                    if (r.Contains(Event.current.mousePosition)) {
                        if (Event.current.type == EventType.MouseDown)
                            callback.Invoke(OrientationLine.Horizontal, x, ResizeMode.Add);
                        
                        if (Event.current.type == EventType.Repaint) {
                            GUIHelper.DrawRectLine(r, Color.green, 4);
                            
                            var position = fieldRect.xMin + x * (slotSize.x + slotOffset) - slotOffset / 2;
                            
                            GUIHelper.DrawLine(
                                new Vector2(position, fieldRect.yMin),
                                new Vector2(position, fieldRect.yMax),
                                Color.green, 4);
                            
                        }
                        return;
                    }
                    
                    r.x += slotSize.x + slotOffset;
                }
            }
            #endregion

            #region Vertical Remove
            if (fieldSize.Y > Level.minSize) {
                r = new Rect(fieldRect.xMin - legendSize.x, fieldRect.yMax - slotSize.y * .75f,
                    legendSize.x, slotSize.y / 2);
                for (int y = visibleArea.down; y <= visibleArea.up; y++) {

                    EditorGUIUtility.AddCursorRect(r, MouseCursor.ArrowMinus);
                    
                    if (r.Contains(Event.current.mousePosition)) {
                        if (Event.current.type == EventType.MouseDown)
                            callback.Invoke(OrientationLine.Vertical, y, ResizeMode.Remove);
                        
                        if (Event.current.type == EventType.Repaint) {
                            GUIHelper.DrawRectLine(r, Color.red, 4);
                            
                            DrawOutlinedShape(Enumerator.For(visibleArea.left, visibleArea.right, 1)
                                .Select(x => new int2(x, y))
                                .ToArray(), Color.red.Transparent(.2f), Color.red);
                        }
                        return;
                    }
                    
                    r.y -= slotSize.y + slotOffset;   
                }
            }

            #endregion
            
            #region Vertical Add
            if (fieldSize.Y < Level.maxSize) {
                r = new Rect(fieldRect.xMin - legendSize.x, fieldRect.yMax - slotSize.y * .25f,
                    legendSize.x, slotSize.y / 2);
                for (int y = visibleArea.down; y <= visibleArea.up + 1; y++) {

                    EditorGUIUtility.AddCursorRect(r, MouseCursor.ArrowPlus);
                    
                    if (r.Contains(Event.current.mousePosition)) {
                        if (Event.current.type == EventType.MouseDown)
                            callback.Invoke(OrientationLine.Vertical, y, ResizeMode.Add);
                        
                        if (Event.current.type == EventType.Repaint) {
                            GUIHelper.DrawRectLine(r, Color.green, 4);
                            
                            var position = fieldRect.yMax - y * (slotSize.y + slotOffset) - slotOffset / 2;
                            
                            GUIHelper.DrawLine(
                                new Vector2(fieldRect.xMin, position),
                                new Vector2(fieldRect.xMax, position),
                                Color.green, 4);
                        }
                        return;
                    }
                    
                    r.y -= slotSize.y + slotOffset;   
                }
            }
            #endregion
        }
        
        List<Vector2> outlineEdges = new List<Vector2>();
        
        public void DrawOutlinedShape(ICollection<int2> coords, Color faceColor, Color outlineColor) {
            if (Event.current.type != EventType.Repaint) return;
            if (faceColor.a <= 0 && outlineColor.a <= 0) return;

            var border = slotOffset / 2;
            var edgeOffset = slotOffset / 5;

            outlineEdges.Clear();

            foreach (var coord in coords) {
                if (TryCoordToSlotRect(coord, out var r)) {
                    
                    r = r.GrowSize(slotOffset);
                    
                    GUIHelper.DrawRect(r, faceColor);
                    
                    if (outlineColor.a > 0) {
                        if (!coords.Contains(coord + Side.Top)) {
                            outlineEdges.Add(new Vector2(r.xMin - edgeOffset, r.yMin));
                            outlineEdges.Add(new Vector2(r.xMax + edgeOffset, r.yMin));
                        }
                        if (!coords.Contains(coord + Side.Bottom)) {
                            outlineEdges.Add(new Vector2(r.xMax + edgeOffset, r.yMax));
                            outlineEdges.Add(new Vector2(r.xMin - edgeOffset, r.yMax));
                        }
                        if (!coords.Contains(coord + Side.Left)) {
                            outlineEdges.Add(new Vector2(r.xMin, r.yMin - edgeOffset));
                            outlineEdges.Add(new Vector2(r.xMin, r.yMax + edgeOffset));
                        }
                        if (!coords.Contains(coord + Side.Right)) {
                            outlineEdges.Add(new Vector2(r.xMax, r.yMin - edgeOffset));
                            outlineEdges.Add(new Vector2(r.xMax, r.yMax + edgeOffset));
                        }
                    }
                }
            }
            if (outlineColor.a > 0)
                for (int i = 0; i < outlineEdges.Count; i += 2)
                    GUIHelper.DrawLine(outlineEdges[i], outlineEdges[i + 1], outlineColor, 4);
        }
        
        internal const float cellSize = 40;
        internal readonly Vector2 legendSize = new Vector2(30, 20);
        internal const float slotOffset = 4;

        Rect CoordToSlotRect(int2 coord) {
            int2 pos = coord;
            return new Rect(fieldRect.xMin + pos.X * (cellSize + slotOffset) + legendSize.x,
                fieldRect.yMin + (fieldSize.Y - pos.Y) * (cellSize + slotOffset) + slotOffset,
                cellSize, 
                cellSize);
        }
    }
    
    
    #region Slot Drawer
    public abstract class SlotContentDrawer {
        static readonly List<SlotContentDrawer> slotDrawers = 
            Utils.FindInheritorTypes<SlotContentDrawer>(true, false)
                .Select(Activator.CreateInstance)
                .Cast<SlotContentDrawer>()
                .OrderBy(d => d.Order)
                .ToList();
        
        public virtual int Order { get; } = 0;
        
        public static void Draw(Rect rect, LevelFieldEditor editor, SlotInfo slotInfo) {
            foreach (SlotContentDrawer slotDrawer in slotDrawers) {
                slotDrawer.DrawSlot(rect, slotInfo, editor);   
                foreach (ContentInfo contentInfo in slotInfo.Content())
                    slotDrawer.DrawSlot(rect, contentInfo, editor);
                foreach (var extension in editor.extensions)
                    slotDrawer.DrawSlot(rect, slotInfo, extension, editor);
            }
        }
        
        public static void PostDraw(LevelFieldEditor editor) {
            foreach (SlotContentDrawer slotDrawer in slotDrawers)
                slotDrawer.PostDrawSlots(editor);
            
        }
        public virtual void PostDrawSlots(LevelFieldEditor editor) {}
        
        public virtual void DrawSlot(Rect rect, SlotInfo slotInfo, LevelFieldEditor editor) {}
        public virtual void DrawSlot(Rect rect, ContentInfo contentDesign, LevelFieldEditor editor) {}
        public virtual void DrawSlot(Rect rect, SlotInfo slotInfo, ContentInfo extension, LevelFieldEditor editor) {}
    }
    #endregion
}