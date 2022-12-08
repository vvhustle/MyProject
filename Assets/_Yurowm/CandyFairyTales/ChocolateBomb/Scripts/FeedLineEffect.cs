using System.Collections;
using UnityEngine;
using YMatchThree.Core;
using Yurowm.ContentManager;
using Yurowm.Extensions;
using Yurowm.Shapes;

namespace Yurowm.Effects {
    public class FeedLineEffect : IEffectLogicProvider {
        public bool IsSuitable(EffectBody effectBody) {
            return effectBody.GetComponent<FeedLineEffectTag>();
        }

        public IEnumerator Logic(LiveContext context, IEffectCallback[] callbacks, Effect effect) {
            if (callbacks.IsEmpty())
                yield break;
            
            var targetProvider = callbacks.CastOne<EffectTargetProvider>();
            if (targetProvider == null)
                yield break;

            if (!effect.body.SetupChildComponent(out YLine2D line))
                yield break;
            
            var completeCallback = callbacks.CastOne<CompleteCallback>();
            
            var interpolator = callbacks.CastOne<NodeEffectInterpolator>() ??
                new NodeEffectLinearInterpolator(5, 0);
            
            interpolator.SetInitialPosition(effect.position);
            
            var point = effect.position;
            
            line.SetPoints(new Vector2[2]);
            
            while (targetProvider.HasTarget()) {
                var targetPosition = targetProvider.GetPosition();
                
                interpolator.OnFrame();
                point = interpolator.GetNextPosition(point, targetPosition);
                
                line.tileY = Vector2.Distance(effect.position, point);
                line.ChangePoint(1, point - effect.position);
                
                if (targetPosition == point) {
                    targetProvider.onReachTarget?.Invoke();
                    break;  
                }

                yield return null;
            }

            completeCallback.onComplete?.Invoke();
            
            var callback = callbacks.CastOne<Callback>();
            
            if (callback != null)
                while (callback.hold)
                    yield return null;

            if (effect.body.SetupComponent(out ContentSound sound))
                sound.Play("Destroying");
            
            if (effect.body.SetupComponent(out ContentAnimator animator))
                yield return animator.PlayAndWait("Destroying");
        }
        
        public class Callback : IEffectCallback {
            public bool hold = false;
        }
    }
}