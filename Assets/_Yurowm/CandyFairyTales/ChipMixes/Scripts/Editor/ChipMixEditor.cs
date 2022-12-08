using Yurowm.ObjectEditors;
using YMatchThree.Core;
using Yurowm.Effects;
using Yurowm.Help;
using Yurowm.Spaces;

namespace Yurowm.Editors {
    public class ChipMixEditor : ObjectEditor<ChipMix> {
        public override void OnGUI(ChipMix mix, object context = null) {
            BaseTypesEditor.SelectAsset<EffectBody>(mix, nameof(mix.effectName));
            EditorTips.PopLastRectByID("lc.base.body");
        }
    }
}