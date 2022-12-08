using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using YMatchThree.Editor;
using Yurowm;
using Yurowm.GUIHelpers;
using Yurowm.Serialization;
using Yurowm.Utilities;

namespace YMatchThree.Core {
    public class TeleportSelectionEditor : ContentSelectionEditor<Teleport> {
        public static ContentInfo[] LastSelection;
        public override void OnGUI(ContentInfo[] selection, LevelFieldEditor fieldEditor) {
            LastSelection = selection;
            
            EUtils.DrawMixedProperty(selection,
                getValue: c => c.GetVariable<CoordinateVariable>().value,
                setValue: (c, value) => c.GetVariable<CoordinateVariable>().value = value,
                drawSingle: (info, value) => DrawSelected(value == int2.Null ? "[null]" : value.ToString(), value, fieldEditor),
                drawMultiple: (value, callback) => {
                    var result = DrawSelected("[multiple values]", int2.Null, fieldEditor);
                    if (result != int2.Null)
                        callback(result);
                });
        }

        int2 clicked = int2.Null;

        int2 DrawSelected(string label, int2 value, LevelFieldEditor fieldEditor) {
            if (GUIHelper.Button("Target", label)) {
                var defaultOnMainClick = fieldEditor.onMainClick;

                void Set(int2 coord) {
                    clicked = coord;
                    fieldEditor.onMainClick = defaultOnMainClick;
                }

                fieldEditor.onMainClick = Set;
            }
            
            if (clicked != int2.Null) {
                value = clicked;
                clicked = int2.Null;
            }
            
            return value;
        }
    }
}