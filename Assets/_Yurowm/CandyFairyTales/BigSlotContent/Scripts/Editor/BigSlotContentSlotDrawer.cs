using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using YMatchThree.Core;
using Yurowm.Extensions;

namespace YMatchThree.Editor {
    public class BigSlotContentSlotDrawer : SlotContentDrawer {
        public override int Order => 10;

        public override void PostDrawSlots(LevelFieldEditor editor) {
            foreach (var slot in editor.slots.Values) {
                foreach (var content in slot.Content()) {
                    if (content.Reference is IBigSlotContent bigSlotContent && content.Reference is SlotContent slotContent) {
                        editor.DrawOutlinedShape(bigSlotContent
                                .GetBigShape()
                                .GetCoords()
                                .Select(c => c + slot.coordinate)
                                .ToArray(),
                            slotContent.color.Transparent(.2f),
                            slotContent.color);
                                
                        if (editor.TryCoordToSlotRect(slot.coordinate, out var rect))
                            GUI.Label(rect, slotContent.shortName, LevelEditorStyles.labelStyle);
                    }
                }
            }
        }
    }
}