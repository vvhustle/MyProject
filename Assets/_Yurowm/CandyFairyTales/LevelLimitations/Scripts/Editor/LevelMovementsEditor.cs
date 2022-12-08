using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using YMatchThree.Core;
using Yurowm;
using Yurowm.Extensions;

namespace YMatchThree.Editor {
    public class LevelMovementsEditor : ContentSelectionEditor<LevelMovements> {
        
        const int min = 5;
        const int max = 99;
        
        const string label = "Count";
        
        public override void OnGUI(ContentInfo[] selection, LevelFieldEditor fieldEditor) {

            EUtils.DrawMixedProperty(selection
                    .Select(s => s.GetVariable<CountVariable>())
                    .NotNull(),
                variable => variable.value,
                (variable, value) => variable.value = value,
                (variable, value) => EditorGUILayout.IntSlider(label, variable.value, min, max));
        }
    }
}


    