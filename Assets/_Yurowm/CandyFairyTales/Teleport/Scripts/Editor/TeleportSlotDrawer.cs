using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using YMatchThree.Editor;
using Yurowm.Extensions;
using Yurowm.GUIHelpers;
using Yurowm.Utilities;

namespace YMatchThree.Core {
    public class TeleportSlotDrawer : SlotContentDrawer {
        
        static readonly Color sourceColor = new Color(0.17f, 0.48f, 1f);
        static readonly Color targetColor = new Color(1f, 0.64f, 0.14f);
        static readonly Color errorColor = new Color(1f, 0.04f, 0f, 0.59f);

        static readonly Vector2 tangent = new Vector2(0, 80);

        public override void PostDrawSlots(LevelFieldEditor editor) {    
            foreach (var slot in editor.slots.Values) {
                foreach (var content in slot.Content()) {
                    if (content.Reference is Teleport) {
                        var variable = content.GetVariable<CoordinateVariable>();
                        
                        bool sourceVisible = false;
                        bool targetVisible = false;
                        
                        if (editor.TryCoordToSlotRect(slot.coordinate, out var sourceRect)) {
                            GUIHelper.DrawRectLine(sourceRect.GrowSize(-5)
                                , variable.value == int2.Null ? errorColor : sourceColor, 4);
                            sourceVisible = true;
                        }
                        
                        if (editor.TryCoordToSlotRect(variable.value, out var targetRect)) {
                            GUIHelper.DrawRectLine(targetRect.GrowSize(-5), targetColor, 4);
                            targetVisible = true;
                        }
                        
                        if (sourceVisible && targetVisible) {
                            var targetTangent = targetRect.center;
                            if (sourceRect.y > targetRect.y)
                                targetTangent += tangent;
                            else
                                targetTangent -= tangent;
                            
                            var sourceTangent = sourceRect.center;
                            if (sourceRect.y > targetRect.y)
                                sourceTangent -= tangent;
                            else
                                sourceTangent += tangent;
                            GUIHelper.DrawBezier(sourceRect.center, targetRect.center,
                                sourceTangent, targetTangent, 
                                sourceColor, targetColor, 5);
                        }
                    }
                }
            }
        }
    }
}