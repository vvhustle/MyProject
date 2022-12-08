using System.Collections;
using System.Linq;
using UnityEngine;
using Yurowm;
using Yurowm.Extensions;
using Yurowm.Utilities;
using Space = Yurowm.Spaces.Space;

namespace YMatchThree.Core {
    public class IngredientTarget : SlotModifier, ISlotRectModifier {
        
        public override void OnAddToSpace(Space space) {
            base.OnAddToSpace(space);
            Reactions.Get(field).Emit<IngredientTargetReaction>();
        }

        public Rect ModifySlotRect(Rect slotRect) {
            var size = Slot.Offset * 0.75f;
            var center = position;
            
            slotRect.yMin = slotRect.yMin.ClampMax(center.y - size);
            
            return slotRect;
        }

        public class IngredientTargetReaction : Reaction {
            public override int GetPriority() {
                return 2;
            }

            public override IEnumerator React() {
                var targets = context.GetAll<IngredientTarget>().ToList();
                
                if (targets.Count == 0) {
                    Kill();
                    yield break;
                }

                var ingredients = targets
                    .Select(s => s.slotModule.Slot().GetContent<Ingredient>())
                    .NotNull()
                    .Where(i => i.IsCollected() && !i.destroying)
                    .ToList();
                
                if (ingredients.Count == 0)
                    yield break;
                
                ingredients.ForEach(l => l.Collect());

                while (ingredients.Count > 0) {
                    yield return null;
                    ingredients.RemoveAll(l => !l.IsAlive() || !l.slotModule.HasSlot());
                }
                
                yield return Reactor.Result.CompleteToGravity;
            }
            
            public override Type GetReactionType() {
                return Type.Gravity | Type.Match;
            }
        }
    }
}