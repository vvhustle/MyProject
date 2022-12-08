using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yurowm.DebugTools;

namespace YMatchThree.Core {
    public class WhiteChip : Chip, IDestroyable, IShuffled {       
        public int scoreReward { get; set; }
        
        public bool IsSuitableForNewSlot(Level level, SlotInfo slot) {
            return true;
        }

        public IEnumerator Destroying() { 
            yield return lcAnimator.PlayClipAndWait("Destroying");
        }
    }
}