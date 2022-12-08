using Yurowm.ObjectEditors;
using UnityEditor;
using UnityEngine;
using Yurowm.Core;
using Yurowm.Dashboard;
using Yurowm.GUIHelpers;
using Yurowm.Serialization;

namespace Yurowm.YPlanets.Editor {
    public class ProjectSettingsEditor : ObjectEditor<ProjectSettings> {
        
        public override void OnGUI(ProjectSettings settings, object context = null) {
            settings.versionName = EditorGUILayout.TextField("Version Name", settings.versionName);
            
            EditorGUILayout.LabelField("Version", settings.Version);
            
            EditorGUILayout.Space();
            settings.appStoreAppID = EditorGUILayout.TextField("Apple AppStore App ID", settings.appStoreAppID);
            
            EditorGUILayout.Space();
            settings.supportEmail = EditorGUILayout.TextField("Support Email", settings.supportEmail);
        }
    }
    
    [DashboardGroup("Content")]
    [DashboardTab("Project", "Puzzle", "tab.project")]
    public class ProjectSettingsStorageEditor : PropertyStorageEditor {
        
        protected override IPropertyStorage EmitNew() {
            return new ProjectSettings();
        }
    }
}