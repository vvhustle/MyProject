using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using YMatchThree.Core;
using Yurowm;
using Yurowm.Effects;
using Yurowm.Extensions;
using Yurowm.Help;
using Yurowm.ObjectEditors;
using Yurowm.Spaces;

namespace YMatchThree.Editor {
    
    public class CollectionGoalEditor : ObjectEditor<CollectionGoal> {
        public override void OnGUI(CollectionGoal goal, object context = null) {
            BaseTypesEditor.SelectAsset<EffectBody>(goal, nameof(goal.collectingEffect));
            EditorTips.PopLastRectByID("lc.collectiongoal.collectingeffect");
        }
    }
    
    public class CollectionGoalSelectedEditor : ContentSelectionEditor<CollectionGoal> {
        
        const int min = 1;
        const int max = 999;
        
        public override void OnGUI(ContentInfo[] selection, LevelFieldEditor fieldEditor) {

            EUtils.DrawMixedProperty(selection
                    .Select(c => c.GetVariable<CollectionSizeVariable>())
                    .NotNull(),
                variable => variable.all,
                (variable, value) => variable.all = value,
                (variable, value) => EditorGUILayout.Toggle("All", variable.all));

            EUtils.DrawMixedProperty(selection
                    .Select(c => c.GetVariable<CollectionSizeVariable>())
                    .NotNull()
                    .Where(c => !c.all),
                variable => variable.count,
                (variable, value) => variable.count = value,
                (variable, value) => EditorGUILayout.IntSlider("Count", variable.count, min, max));
        }
    }
}