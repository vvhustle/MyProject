using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YMatchThree.Core;
using Yurowm;
using Yurowm.Extensions;
using Yurowm.Utilities;
using Behaviour = Yurowm.Behaviour;

namespace YMatchThree.UI {
    public class StarNotch : Behaviour {
        
        ContentAnimator animator;
        
        public int index = 1;

        int targetScore = 0;
        bool fill = false;

        public override void Initialize() {
            base.Initialize();
            this.SetupComponent(out animator);
        }

        public void Place(LevelStars stars) {
            fill = false;
            targetScore = stars.GetValue(index);
            
            animator.RewindEnd("Rollback");
            
            var parent = transform.parent.rect();
            
            if (!parent) return;
            
            float value = 1f * targetScore / stars.Third;
                
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.zero;
            
            rectTransform.anchoredPosition = new Vector2(parent.rect.width * value, 0);
        }
        
        public void OnChangeScore(int score) {
            bool newState = score >= targetScore;
            
            if (newState == fill) return;
            
            fill = newState;
            
            animator.Play(fill ? "Fill" : "Rollback");
        }
    }
}