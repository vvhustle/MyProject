using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using YMatchThree.Core;
using Yurowm.GUIHelpers;
using Yurowm.Help;
using Yurowm.Icons;
using Yurowm.ObjectEditors;
using Yurowm.Spaces;

namespace YMatchThree.Editor {
    
    public class BlockEditor : ObjectEditor<Block> {
        public override void OnGUI(Block block, object context = null) {
            block.blockingType = (Block.BlockingType) EditorGUILayout.EnumPopup("Blocking Type", block.blockingType);
            EditorTips.PopLastRectByID("lc.block.blockingtype");
        }
    }
    
    public class BlockSlotDrawer : SlotContentDrawer {
        Texture2D icon;

        public override int Order => 2;

        public override void DrawSlot(Rect rect, ContentInfo info, LevelFieldEditor editor) {
            if (info.Reference is Block block) {
                if (!icon)
                    icon = EditorIcons.GetIcon("BlockIcon");
        
                var colored = info.GetVariable<ColoredVariable>();
                if (colored != null && colored.info.type == ItemColor.Known) {
                    using (GUIHelper.Color.Start(Color.Lerp(ItemColorEditorPalette.Get(colored.info.ID), Color.white, 0.4f)))
                        GUI.DrawTexture(rect, icon);
                } else
                    GUI.DrawTexture(rect, icon);
                
                var layered = info.GetVariable<LayeredVariable>();
                
                rect.height /= 2;
                rect.y += rect.height;
                
                GUI.Label(rect, block.shortName + (layered != null ? $":{layered.count}" : ""), LevelEditorStyles.labelStyle);
            }
        }
    }
}