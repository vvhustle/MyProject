using Yurowm.ObjectEditors;
using UnityEditor;
using YMatchThree.Core;

namespace Yurowm.Editors {
    public class LadybirdEditor : ObjectEditor<Ladybird> {
        public override void OnGUI(Ladybird ladybird, object context = null) {
            Edit("Move Explosion", ladybird.moveExplosion);
        }
    }
}