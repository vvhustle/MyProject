using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using YMatchThree.Core;
using Yurowm.Extensions;
using Yurowm.Utilities;

namespace YMatchThree.Editor {
    public class WallsSlotDrawer : SlotContentDrawer {
        
        static readonly Color wallIconColor = new Color(.7f, .5f, .2f, .7f);
        static readonly Color wallShadowColor = new Color(0, 0, 0, .3f);

        static readonly Side[] wallSides = {
            Side.Top,
            Side.Left
        };

        public override void DrawSlot(Rect rect, SlotInfo slotInfo, ContentInfo extension, LevelFieldEditor editor) {
            if (extension.Reference is Walls) {
                var info = extension.GetVariable<WallsVariable>();
                
                foreach (Side side in wallSides) {
                    if (Walls.WallInfo.IsWall(slotInfo.coordinate, side, editor.slots.Keys, info)) {
                        Rect wRect;
                        
                        if (side.X() == 0)
                            wRect = new Rect(rect.x, rect.y - LevelFieldEditor.slotOffset, rect.width, LevelFieldEditor.slotOffset);
                        else
                            wRect = new Rect(rect.x - LevelFieldEditor.slotOffset, rect.y, LevelFieldEditor.slotOffset, rect.height);
                        
                        Handles.DrawSolidRectangleWithOutline(wRect, wallIconColor, wallShadowColor);
                    }
                }
            }
        }
    }
}