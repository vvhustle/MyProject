using System.Collections;
using Yurowm.Coroutines;

namespace YMatchThree.Core {
    public class Ingredient : Chip, IDestroyable {

        public int scoreReward { get; set; }

        public bool IsCollected() {
            return slotModule.Slot().HasContent<IngredientTarget>();   
        }
        
        protected override bool CanBeHit(HitContext hitContext) {
            return IsCollected();
        }

        public virtual IEnumerator Destroying() {
            yield return Collecting();
        }
        
        public void Collect() {
            if (scoreReward > 0) { 
                gameplay.score.AddScore(scoreReward);
                ShowScoreEffect(scoreReward, colorInfo);
            }
            events.onStartDestroying.Invoke(this, new HitContext(context, slotModule.Slot(), HitReason.Reaction));
            Collecting().Run(field.coroutine);
        }
        
        protected virtual IEnumerator Collecting() {
            yield return lcAnimator.PlayClipAndWait("Destroying");
            BreakParent();
        }
    }
}    