using Yurowm.ObjectEditors;
using UnityEditor;
using YMatchThree.Core;
using Yurowm.GUIHelpers;
using Yurowm.Help;

namespace Yurowm.Editors {
    public class ExplosionParametersEditor : ObjectEditor<ExplosionParameters> {
        public override void OnGUI(ExplosionParameters parameters, object context = null) {
            parameters.force = EditorGUILayout.FloatField("Force", parameters.force);
            EditorTips.PopLastRectByID("lc.explosion.force");
            
            parameters.radius = EditorGUILayout.FloatField("Radius", parameters.radius).ClampMin(1);
            EditorTips.PopLastRectByID("lc.explosion.radius");
        }
    }
}