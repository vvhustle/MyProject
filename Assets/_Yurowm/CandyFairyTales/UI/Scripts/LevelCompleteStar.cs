using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yurowm;
using Yurowm.Extensions;
using Behaviour = Yurowm.Behaviour;

namespace YMatchThree.Core {
    public class LevelCompleteStar : Behaviour {
        
        public int number = 1;

        public void No() {
            if (this.SetupComponent(out ContentAnimator animator)) 
                animator.Rewind("Award");
        }
        
        public void Award() {
            this.PlayClip("Award");
        }
    }
}