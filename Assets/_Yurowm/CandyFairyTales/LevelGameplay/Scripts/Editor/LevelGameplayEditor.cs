using UnityEditor;
using YMatchThree.Core;
using Yurowm.Effects;
using Yurowm.GUIHelpers;
using Yurowm.ObjectEditors;
using Yurowm.Spaces;

namespace YMatchThree.Editor {
    public class LevelGameplayEditor : ObjectEditor<LevelGameplay> {
        public override void OnGUI(LevelGameplay gameplay, object context = null) {

            BaseTypesEditor.SelectAsset<SlotsBody>(gameplay, nameof(gameplay.slotRenderer));
            BaseTypesEditor.SelectAsset<EffectBody>(gameplay, nameof(gameplay.scoreEffect));
            BaseTypesEditor.SelectAsset<EffectBody>(gameplay, nameof(gameplay.slotHighlighter));

            gameplay.hintDelay = EditorGUILayout.Slider("Hint Delay", gameplay.hintDelay, 0f, 30f);
            gameplay.nullSlotBlockingType = (Block.BlockingType) EditorGUILayout.EnumPopup("Null Slot Blocking Type", gameplay.nullSlotBlockingType);
            
            EditorGUILayout.LabelField("Shuffle Feedback");
            using (GUIHelper.IndentLevel.Start()) 
                Edit(gameplay.shuffleText);
            
            EditorGUILayout.LabelField("Fail Reason");
            using (GUIHelper.IndentLevel.Start()) 
                Edit(gameplay.shuffleFailReason);
        }
    }
}