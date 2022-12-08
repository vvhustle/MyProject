using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using YMatchThree.Editor;
using Yurowm;
using Yurowm.Extensions;
using Yurowm.GUIHelpers;
using Yurowm.GUIStyles;
using Yurowm.Utilities;

namespace YMatchThree.Core {
    public class WallsSelectionEditor : LevelSlotExtensionSelectionEditor<Walls> {
        
        public override void OnGUI(SlotInfo[] slots, ContentInfo extension, LevelFieldEditor fieldEditor) {
            DrawOutlineGUI(slots, extension, fieldEditor);
            DrawSidesGUI(slots, extension, fieldEditor);
        }

        #region Outline

        void DrawOutlineGUI(SlotInfo[] slots, ContentInfo extension, LevelFieldEditor fieldEditor) {
            Rect rect = EditorGUILayout.GetControlRect(GUILayout.ExpandWidth(true), GUILayout.Height(EditorGUIUtility.singleLineHeight));

            if (Event.current.type == EventType.Layout) return;

            Rect rect2 = EditorGUI.PrefixLabel(rect, new GUIContent("Outline"));
            rect.xMin = rect2.x;

            int exist = 0;
            int notexist = 0;
            
            var info = extension.GetVariable<WallsVariable>();
            
            foreach (var slot in slots) {
                foreach (var side in Sides.straight)
                    if (fieldEditor.slots.ContainsKey(slot.coordinate + side) 
                        && slots.All(s => s.coordinate != slot.coordinate + side)) {
                        
                        if (Walls.WallInfo.IsWall(slot.coordinate, side, fieldEditor.slots.Keys, info)) 
                            exist++; 
                        else
                            notexist++;
                    }
            }

            if (exist > 0 && notexist > 0) {
                if (GUI.Button(rect, "[Set]", EditorStyles.miniButton)) SetOutline(true, slots, fieldEditor, info);
            } else if (exist > 0) {
                if (GUI.Button(rect, "Remove", EditorStyles.miniButton)) SetOutline(false, slots, fieldEditor, info);
            } else if (notexist > 0) {
                if (GUI.Button(rect, "Set", EditorStyles.miniButton)) SetOutline(true, slots, fieldEditor, info);
            } else {
                GUI.Button(rect, "-", EditorStyles.miniButton);
            }
        }

        void SetOutline(bool value, SlotInfo[] slots, LevelFieldEditor fieldEditor, WallsVariable info) {
            foreach (var slot in slots) {
                if (!fieldEditor.slots.ContainsKey(slot.coordinate)) continue;
                foreach (Side side in Sides.straight)
                    if (slots.All(s => s.coordinate != slot.coordinate + side))
                        SetWall(value, slot.coordinate, side, fieldEditor.slots.Keys, info);
            }
        }

        #endregion

        #region Sides
        
        GUIContent multipleValuesSideContent = new GUIContent("?");

        void DrawSidesGUI(SlotInfo[] slots, ContentInfo extension, LevelFieldEditor fieldEditor) {
            var rect = EditorGUILayout.GetControlRect(GUILayout.ExpandWidth(true), GUILayout.Height(80));   
            
            if (Event.current.type == EventType.Layout) return;
            
            var info = extension.GetVariable<WallsVariable>();
            
            rect = EditorGUI.PrefixLabel(rect, new GUIContent("Sides"));
            
            rect.width = rect.width.ClampMax(rect.height);

            var buttonSize = EditorGUIUtility.singleLineHeight;

            EditSide(Side.Left, new Rect(rect.xMin, rect.yMin + buttonSize, buttonSize, rect.height - buttonSize * 2));
            EditSide(Side.Right, new Rect(rect.xMax - buttonSize, rect.yMin + buttonSize, buttonSize, rect.height - buttonSize * 2));
            EditSide(Side.Bottom, new Rect(rect.xMin + buttonSize, rect.yMax - buttonSize, rect.width - buttonSize * 2, buttonSize));
            EditSide(Side.Top, new Rect(rect.xMin + buttonSize, rect.yMin, rect.width - buttonSize * 2, buttonSize));
            
            void EditSide(Side side, Rect rect) {
                EUtils.DrawMixedProperty(slots
                        .Where(s => fieldEditor.slots.ContainsKey(s.coordinate + side)),
                    slot => Walls.WallInfo.IsWall(slot.coordinate, side, fieldEditor.slots.Keys, info),
                    (slot, value) => {
                        if (value != Walls.WallInfo.IsWall(slot.coordinate, side, fieldEditor.slots.Keys, info))
                            SetWall(value, slot.coordinate, side, fieldEditor.slots.Keys, info);
                    },
                    (position, value) => 
                        GUI.Toggle(rect, value, (string) null, Styles.miniButton),
                    (value, callback) => {
                            if (GUI.Toggle(rect, false, multipleValuesSideContent, Styles.miniButton))
                                callback(true);
                    }
                );
            }
        }
        
        
        
        #endregion
        
        void SetWall(bool value, int2 coord, Side side, ICollection<int2> slots, WallsVariable info) {
            var wall = Walls.WallInfo.GetWall(coord, side, slots);

            if (!wall.exist) return;

            var orientation = info.orientations.Get(wall.coord);

            var orientationRef = wall.orientation;

            if (orientation.HasFlag(orientationRef) == value) return;

            if (value)
                orientation = orientation | orientationRef;
            else
                orientation = orientation & ~orientationRef;
            
            if (orientation == 0)
                info.orientations.Remove(wall.coord);
            else
                info.orientations[wall.coord] = orientation;
        }
    }
}