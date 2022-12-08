using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YMatchThree.Core;
using Yurowm.ContentManager;
using Yurowm.Help;
using Yurowm.ObjectEditors;
using Yurowm.Spaces;

namespace YMatchThree.Editor {
    public class LayoutBlockEditor : ObjectEditor<LayoutBlock> {
        public override void OnGUI(LayoutBlock block, object context = null) {
            BaseTypesEditor.SelectAsset<SlotsBody>(block, nameof(block.slotRenderer));
            EditorTips.PopLastRectByID("lc.layoutblock.renderer");
        }
    }
}