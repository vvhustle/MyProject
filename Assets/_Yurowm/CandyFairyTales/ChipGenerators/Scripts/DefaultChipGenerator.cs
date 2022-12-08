using Yurowm.Extensions;
using Yurowm.Serialization;
using Yurowm.Utilities;
using Space = Yurowm.Spaces.Space;

namespace YMatchThree.Core {
    [SlotTag]
    [SerializeShort]
    public class DefaultChipGenerator : ChipGenerator, IDefaultSlotContent {
        public bool IsSuitableForNewSlot(Level level, SlotInfo slot) {
            return slot.coordinate.Y == level.height - 1;
        }

        ContentInfo defaultChipInfo;

        public override void OnAddToSpace(Space space) {
            base.OnAddToSpace(space);
            
            var defaultChip = storage
                .GetDefault<SimpleChip>();
            
            defaultChipInfo = new ContentInfo(defaultChip);
        }

        public override ContentInfo GetNextChipInfo() {
            return defaultChipInfo;
        }
    }
}