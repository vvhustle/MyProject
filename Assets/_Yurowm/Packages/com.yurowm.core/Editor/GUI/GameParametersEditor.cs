using Yurowm.ObjectEditors;
using UnityEditor;
using UnityEngine;
using Yurowm.Dashboard;
using Yurowm.GUIHelpers;
using Yurowm.GUIStyles;
using Yurowm.Serialization;

namespace Yurowm.YPlanets.Editor {
    public class GameParametersEditor : ObjectEditor<GameParameters> {
        public override void OnGUI(GameParameters parameters, object context = null) {
            foreach (var module in parameters.GetModules()) {
                GUILayout.Label(module.GetName(), Styles.miniTitle);
                using (GUIHelper.IndentLevel.Start()) 
                    Edit(module);
            }
        }
    }
    
    public class GameParametersGeneralEditor : ObjectEditor<GameParametersGeneral> {
        public override void OnGUI(GameParametersGeneral parameters, object context = null) {
            parameters.privacyPolicyURL = EditorGUILayout.TextField("Private Policy URL", parameters.privacyPolicyURL);
            parameters.maxDeltaTime = EditorGUILayout.FloatField("Delta Time (Max)", parameters.maxDeltaTime).ClampMin(1f / 60);
        }
    }

    [DashboardGroup("Content")]
    [DashboardTab("Game", null, "tab.game")]
    public class GameParametersStorageEditor : PropertyStorageEditor {
        protected override IPropertyStorage EmitNew() {
            return new GameParameters();
        }
    }
}