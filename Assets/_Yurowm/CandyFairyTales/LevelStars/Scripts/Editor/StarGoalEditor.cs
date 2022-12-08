using UnityEditor;
using Yurowm.ObjectEditors;

namespace YMatchThree.Core {
    public class StarGoalEditor : ObjectEditor<StarGoal> {
        public override void OnGUI(StarGoal goal, object context = null) {
            goal.starCount = EditorGUILayout.IntSlider("Star Count", goal.starCount, 1, 3);
        }
    }
}