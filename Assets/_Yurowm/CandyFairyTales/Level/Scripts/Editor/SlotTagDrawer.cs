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
    public class SlotTagDrawer : SlotContentDrawer {
        static Dictionary<Type, SlotTagAttribute> tags;
        
        static SlotTagDrawer() {
            tags = Utils.FindInheritorTypes<SlotContent>(true, false)
                .Where(t => t.GetAttribute<SlotTagAttribute>() != null)
                .ToDictionaryValue(t => t.GetAttribute<SlotTagAttribute>());
        }
        
        public SlotTagDrawer() {}

        public override int Order => int.MaxValue;
        
        static Rect[] tagRects = new Rect[] {
            new Rect(0, 0, .25f, .25f),
            new Rect(.25f, 0, .25f, .25f),
            new Rect(.5f, 0, .25f, .25f),
            new Rect(.75f, 0, .25f, .25f),
            new Rect(0, .25f, .25f, .25f),
            new Rect(.75f, .25f, .25f, .25f),
        };

        public override void DrawSlot(Rect rect, SlotInfo slotInfo, LevelFieldEditor editor) {
            int tagIndex = 0;

            foreach (var contentInfo in slotInfo.Content()) {
                if (tags.TryGetValue(contentInfo.Reference.GetType(), out var tag)) {
                    
                    var slotContent = contentInfo.Reference as SlotContent;  
                    
                    Rect tagRect = new Rect(
                        rect.x + tagRects[tagIndex].x * rect.width,
                        rect.y + tagRects[tagIndex].y * rect.height,
                        rect.width * tagRects[tagIndex].width,
                        rect.height * tagRects[tagIndex].height);
                    
                    if (!DrawTag(tagRect, slotContent.shortName, tag.GetSymbolColor(), tag.GetBackgroundColor())) 
                        continue;
                        
                    tagIndex++;
                    
                    if (tagIndex >= tagRects.Length)
                        break;
                }
            }
        }
        
        bool DrawTag(Rect rect, string symbol, Color color, Color background) {
            if (symbol.IsNullOrEmpty()) return false;
            Handles.DrawSolidRectangleWithOutline(rect, background, Color.clear);
            using (GUIHelper.Color.Start(color))
                GUI.Box(rect, symbol[0].ToString(), Styles.tagLabel);
            return true;
        }
    }
}