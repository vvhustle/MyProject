using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using YMatchThree.Core;
using Yurowm.Help;
using Yurowm.ObjectEditors;

namespace YMatchThree.Editor {
    public class ExplosionMixEditor : ObjectEditor<ExplosionMix> {
        public override void OnGUI(ExplosionMix mix, object context = null) {
            Edit("Hit", mix.hit);
            EditorTips.PopLastRectByID("lc.explosionmix.hit");
        }
    }
}