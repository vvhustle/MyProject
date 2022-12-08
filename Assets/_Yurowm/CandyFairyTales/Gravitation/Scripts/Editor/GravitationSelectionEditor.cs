using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using YMatchThree.Editor;
using Yurowm;
using Yurowm.Extensions;
using Yurowm.Utilities;

namespace YMatchThree.Core {
    public class GravitationSelectionEditor : LevelSlotExtensionSelectionEditor<Gravitation> {
        
        static readonly Side[] sides = {
            Side.Top,
            Side.Right,
            Side.Bottom,
            Side.Left
        };
        
        static readonly Dictionary<Side, string> symbols = new Dictionary<Side, string>() {
            { Side.Top, "^"},
            { Side.Right, ">"},
            { Side.Bottom, "v"},
            { Side.Left, "<"}
        };
        
        public override void OnGUI(SlotInfo[] slots, ContentInfo extension, LevelFieldEditor fieldEditor) {
            Rect rect = EditorGUILayout.GetControlRect(GUILayout.ExpandWidth(true), GUILayout.Height(EditorGUIUtility.singleLineHeight));

            if (Event.current.type == EventType.Layout) return;

            Rect rect2 = EditorGUI.PrefixLabel(rect, new GUIContent("Direction"));
            rect.xMin = rect2.x;

            Rect buttonRect = new Rect(rect);
            buttonRect.width /= sides.Length;

            GUIStyle style;        

            var info = extension.GetVariable<EachSlotGravitationVariable>();
            
            for (int i = 0; i < sides.Length; i++) {
                Side side = sides[i];

                if (i == 0)
                    style = EditorStyles.miniButtonLeft;
                else if (i == sides.Length - 1)
                    style = EditorStyles.miniButtonRight;
                else
                    style = EditorStyles.miniButtonMid;

                EUtils.DrawMixedProperty(slots,
                    mask: slot => true,
                    getValue: slot => info.directions.Get(slot.coordinate).HasFlag(side),
                    setValue: (slot, value) => {
                        var current = info.directions.Get(slot.coordinate);
                        info.directions[slot.coordinate] = value ? current | side : current & ~side;
                    },
                    drawSingle: (coord, value) => GUI.Toggle(buttonRect, value, symbols[side], style),
                    drawMultiple: (value, callback) => {
                        if (GUI.Toggle(buttonRect, value, $"[{symbols[side]}]", style) != value)
                            callback(!value);
                    },
                    drawEmpty: () => GUI.Toggle(buttonRect, false, "-", style)
                );
                buttonRect.x += buttonRect.width;
            }
            
            
            
            
        }
    }
}