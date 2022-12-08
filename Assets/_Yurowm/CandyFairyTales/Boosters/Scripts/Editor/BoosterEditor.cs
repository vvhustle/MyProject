using UnityEngine;
using Yurowm.ObjectEditors;
using YMatchThree.Core;
using Yurowm.Spaces;

namespace YMatchThree.Editor {
    public class BoosterEditor : ObjectEditor<Booster> {
        public override void OnGUI(Booster booster, object context = null) {
            BaseTypesEditor.SelectAsset<Sprite>("Icon", booster, nameof(booster.iconName));
            BaseTypesEditor.SelectAsset<BoosterButton>("Button", booster, nameof(booster.buttonName));
            BaseTypesEditor.SelectAsset<BoosterAnimationBody>("Animation", booster, nameof(booster.animationName));
        }
    }
}