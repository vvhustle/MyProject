using UnityEditor;
using YMatchThree.Core;
using Yurowm.Help;
using Yurowm.ObjectEditors;
using Yurowm.Utilities;

namespace YMatchThree.Editor {
    public class LineExplosionMixEditor : ObjectEditor<LineExplosionMix> {
        public override void OnGUI(LineExplosionMix mix, object context = null) {
            Edit("Hit", mix.hit);
        }
    }
}