using System.Collections;
using System.Linq;
using Yurowm.Extensions;
using Space = Yurowm.Spaces.Space;

namespace YMatchThree.Core {
    public class BlockChip : Chip, INearHitDestroyable, ILineBreaker {

        #region IDestroyable
        
        public int scoreReward { get; set; }
        
        public virtual IEnumerator Destroying() {
            BreakParent();
            yield return lcAnimator.PlayClipAndWait("Destroying");
        }

        #endregion
    }
}