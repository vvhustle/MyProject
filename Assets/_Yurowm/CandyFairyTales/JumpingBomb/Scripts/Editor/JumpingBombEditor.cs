using Yurowm.ObjectEditors;
using UnityEditor;
using YMatchThree.Core;
using Yurowm.Effects;
using Yurowm.Spaces;

namespace Yurowm.Editors {
    public class JumpingBombEditor : ObjectEditor<JumpingBomb> {
        public override void OnGUI(JumpingBomb bomb, object context = null) {
            BaseTypesEditor.SelectAsset<EffectBody>(bomb, nameof(bomb.jumpingEffectName));
            Edit("Explosion (Start)", bomb.startExplosion);
            Edit("Explosion (End)", bomb.endExplosion);
        }
    }
}