using System;

namespace YMatchThree.Core {
    [BaseContentOrder(3)]
    public abstract class SlotModifier : SlotContent {

        public override Type GetContentBaseType() {
            return typeof(SlotModifier);
        }

        public override bool IsUniqueContent() {
            return false;
        }
    }
}