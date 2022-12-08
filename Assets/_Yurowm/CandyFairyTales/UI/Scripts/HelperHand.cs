using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Yurowm;
using Yurowm.ContentManager;
using Yurowm.Coroutines;
using Yurowm.Extensions;
using Yurowm.Utilities;

namespace YMatchThree.Core {
    public class HelperHand : ContextedBehaviour, IReserved {
        
        public void Animate(MatchThreeGameplay.Move move, Field field) {
            Animation(move, field).Run(field.coroutine);
        }
        
        IEnumerator Animation(MatchThreeGameplay.Move move, Field field) {
            if (!field.slots.all.TryGetValue(move.A, out var slotA)) yield break;
            if (!field.slots.all.TryGetValue(move.B, out var slotB)) yield break;
            
            if (slotA.GetAllContent().Any(c => move.solution.contents.Contains(c))) {
                var z = slotB;
                slotB = slotA;
                slotA = z;
            }

            transform.localPosition = slotA.position;
            
            var nextTurn = false;
            
            void OnNextTurn() {
                nextTurn = true;
            }
            
            field.gameplay.onNextTurn += OnNextTurn;
            
            this.SetupComponent(out ContentAnimator animator);
            this.SetupComponent(out AnimationSampler sampler);
            animator?.Play("Show");

            var time = field.time;
            var t = 0f;
            
            var speed = 1f;
            if (sampler)
                speed = 1f / sampler.Length;
            while (!nextTurn) {
                
                transform.localPosition = Vector2.Lerp(slotA.position, slotB.position,
                    Mathf.PingPong(t, 1).Ease(EasingFunctions.Easing.InOutCubic));
                
                if (sampler)
                    sampler.Time = Mathf.Repeat(t / 2, 1);

                t += time.Delta * speed / 2;
                
                yield return null;
            }
            
            field.gameplay.onNextTurn -= OnNextTurn;
            yield return animator?.PlayAndWait("Hide");
            
            Kill();
        }

        public void Rollout() { }
    }
}