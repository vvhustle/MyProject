using UnityEngine;
using Yurowm.Extensions;
using Yurowm.Serialization;
using Yurowm.Spaces;

namespace YMatchThree.Core {
    [SerializeShort]
    public class SimpleChip : WhiteChip, IColored, IDefaultSlotContent, IContentWithOverlay {
        public override SpaceObject EmitBody() {
            var generalChipBody = context.GetArgument<LevelScriptBase>().chipBody;
            
            if (!generalChipBody.IsNullOrEmpty())
                bodyName = generalChipBody;
            
            return base.EmitBody();
        }

        public void SetOverlay(Sprite sprite) {
            ContentOverlay overlay = null;
            body?.SetupChildComponent(out overlay); 
            overlay?.SetOverlay(sprite);
        }
    }
}