using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using YMatchThree.Core;
using Yurowm;
using Yurowm.Help;
using Yurowm.ObjectEditors;
using Yurowm.Spaces;

namespace YMatchThree.Editor {
    public class BombChipBaseEditor : ObjectEditor<BombChipBase> {
        public override void OnGUI(BombChipBase chip, object context = null) {
            BaseTypesEditor.SelectAsset<Sprite>("Overlay", chip, nameof(chip.overlayID));    
            EditorTips.PopLastRectByID("lc.bomb.overlay");
            
            chip.activationType = (BombActivationType) EditorGUILayout.EnumFlagsField("Activation Type", chip.activationType);
            EditorTips.PopLastRectByID("lc.bomb.activation");
        }
    }
}