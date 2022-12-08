using System.Linq;
using UnityEditor;
using YMatchThree.Editor;
using Yurowm;
using Yurowm.Extensions;

namespace YMatchThree.Core {
    public class CombinationsGoalSelectedEditor : ContentSelectionEditor<CombinationsGoal> {
        
        public override void OnGUI(ContentInfo[] selection, LevelFieldEditor fieldEditor) {

            EUtils.DrawMixedProperty(selection
                    .Select(c => c.GetVariable<CollectionSizeVariable>())
                    .NotNull()
                    .Where(c => !c.all),
                variable => variable.count,
                (variable, value) => variable.count = value,
                (variable, value) => EditorGUILayout.IntField("Count", variable.count).ClampMin(1));
        }
    }
    
}