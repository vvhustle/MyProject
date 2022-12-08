using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using YMatchThree.Core;
using Yurowm.Extensions;
using Yurowm.Utilities;

namespace YMatchThree.Editor {
    public class GravitationSlotDrawer : SlotContentDrawer {
        static readonly Color directionIconColor = new Color(.5f, .9f, .2f, 1f);
        static readonly Color directionShadowColor = new Color(.4f, .5f, .1f, .3f);
        
        public override int Order => 100;

        public override void DrawSlot(Rect rect, SlotInfo slotInfo, ContentInfo extension, LevelFieldEditor editor) {
            if (extension.Reference is Gravitation) {
                var info = extension.GetVariable<EachSlotGravitationVariable>();
                
                foreach (var side in Sides.straight) {
                    if (info.directions.Get(slotInfo.coordinate).HasFlag(side)) {

                        Rect wRect = new Rect();
                        
                        if (side.X() == 0) {
                            wRect.width = rect.width * 2 / 3;
                            wRect.height = 2;
                            wRect.x = rect.center.x - wRect.width / 2;
                            wRect.y = side.Y() < 0 ? rect.yMax - wRect.height : rect.yMin;
                        } else {
                            wRect.width = 2;
                            wRect.height = rect.height * 2 / 3;
                            wRect.x = side.X() > 0 ? rect.xMax - wRect.width : rect.xMin;
                            wRect.y = rect.center.y - wRect.height / 2;
                        }

                        Handles.DrawSolidRectangleWithOutline(wRect, directionIconColor, directionShadowColor);
                    }
                }
            }
        }
    }
}