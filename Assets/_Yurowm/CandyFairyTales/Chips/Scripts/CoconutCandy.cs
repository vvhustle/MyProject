using Yurowm.Spaces;

namespace YMatchThree.Core {
    public class CoconutCandy : WhiteChip, IColored {
        public override void OnAddToSpace(Space space) {
            colorInfo = ItemColorInfo.Universal;
            base.OnAddToSpace(space);
        }
    }
}