using System;
using Yurowm.ObjectEditors;
using UnityEditor;

namespace Yurowm.Features {
    public class LifeSystemEditor : ObjectEditor<LifeSystem> {
        public override void OnGUI(LifeSystem system, object context = null) {
            system.lifeCount = EditorGUILayout.IntField("Life Count", system.lifeCount).ClampMin(1);
            
            if (TimeSpan.TryParse(EditorGUILayout.TextField("Recovery Time", system.lifeRecoveryTimeSpan.ToString()), out var value))
                system.lifeRecoveryTimeSpan = value;
        }
    }
}