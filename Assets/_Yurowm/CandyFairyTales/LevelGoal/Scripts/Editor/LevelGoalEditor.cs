using UnityEditor;
using YMatchThree.Core;
using Yurowm.GUIHelpers;
using Yurowm.ObjectEditors;

namespace YMatchThree.Editor {
    public class LevelGoalEditor : ObjectEditor<LevelGoal> {
        public override void OnGUI(LevelGoal goal, object context = null) {
			Edit("Fail Reason", goal.failReason);
        }
    }
}