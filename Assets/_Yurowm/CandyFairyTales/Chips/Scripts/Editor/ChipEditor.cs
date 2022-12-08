using UnityEngine;
using YMatchThree.Core;
using Yurowm.GUIHelpers;
using Yurowm.Icons;

namespace YMatchThree.Editor {
    
    public class ChipSlotDrawer : SlotContentDrawer {
        
        Texture2D icon;
        
        public override int Order => 1;
        
        public override void DrawSlot(Rect rect, ContentInfo info, LevelFieldEditor editor) {
            if (info.Reference is Chip chip && GetColor(info, out var color)) {
                if (!icon)
                    icon = EditorIcons.GetIcon("ChipIcon");
        
                using (GUIHelper.Color.Start(color))
                    GUI.DrawTexture(rect, icon);
                
                var layered = info.GetVariable<LayeredVariable>();
                
                GUI.Label(rect, chip.shortName + (layered != null ? $":{layered.count}" : ""), LevelEditorStyles.labelStyle);
            }
        }
        
        public static bool GetColor(ContentInfo info, out Color color) {
            if (info.Reference is SlotContent slotContent) {
                var colored = info.GetVariable<ColoredVariable>();
                if (colored != null && colored.info.type == ItemColor.Known)
                    color = Color.Lerp(ItemColorEditorPalette.Get(colored.info.ID), Color.white, 0.4f);
                else
                    color = slotContent.color;
                
                return true;
            }
            color = default;
            return false;
        }
    }
}