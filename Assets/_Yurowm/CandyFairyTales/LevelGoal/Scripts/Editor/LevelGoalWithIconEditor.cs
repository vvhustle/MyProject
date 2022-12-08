using UnityEngine;
using YMatchThree.Core;
using Yurowm.ObjectEditors;
using Yurowm.Spaces;

namespace YMatchThree.Editor {
    public class LevelGoalWithIconEditor : ObjectEditor<LevelGoalWithIcon> {
        public override void OnGUI(LevelGoalWithIcon goal, object context = null) {
            BaseTypesEditor.SelectAsset<Sprite>(goal, nameof(goal.iconName));
        }
    }
}