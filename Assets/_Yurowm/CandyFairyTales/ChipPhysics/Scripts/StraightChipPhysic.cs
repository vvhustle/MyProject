using System.Linq;
using Yurowm.Serialization;
using Yurowm.Utilities;

namespace YMatchThree.Core {  
    [SerializeShort]  
    public class StraightChipPhysic : ChipPhysic {
        public override Slot GetFallingSlot(Slot slot) {
            return slot.falling.Values.Where(IsAvailableForFalling).GetRandom(random);
        }
    }
}