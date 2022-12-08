using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using YMatchThree.Core;
using Yurowm.ContentManager;
using Yurowm.Coroutines;
using Yurowm.Extensions;

namespace Yurowm.Effects {
    public class NodeEffect : IEffectLogicProvider {
        
        public bool IsSuitable(EffectBody effectBody) {
            return effectBody.GetComponent<NodeEffectTag>();
        }

        public IEnumerator Logic(LiveContext context, IEffectCallback[] callbacks, Effect effect) {
            if (callbacks.IsEmpty())
                yield break;
            
            var targetProvider = callbacks.CastOne<EffectTargetProvider>();
            if (targetProvider == null)
                yield break;
            
            var callback = callbacks.CastOne<CompleteCallback>();
            
            var interpolator = callbacks.CastOne<NodeEffectInterpolator>() ??
                new NodeEffectLinearInterpolator(5, 0);
            
            interpolator.SetInitialPosition(effect.position);
            
            while (targetProvider.HasTarget()) {
                var targetPosition = targetProvider.GetPosition();
                
                interpolator.OnFrame();
                
                effect.position = interpolator.GetNextPosition(effect.position, targetPosition);

                if (targetPosition == effect.position) {
                    targetProvider.onReachTarget?.Invoke();
                    break;  
                }

                yield return null;
            }

            callback.onComplete?.Invoke();
            if (effect.body.SetupComponent(out ContentAnimator animator))
                animator.Play("Destroying");
            if (effect.body.SetupComponent(out ContentSound sound))
                sound.Play("Destroying");

            var trails = effect.body.GetComponentsInChildren<TrailRenderer>();
            
            if (trails.Length > 0)
                yield return effect.time.Wait(trails.Max(t => t.time));
        }
    }
}
