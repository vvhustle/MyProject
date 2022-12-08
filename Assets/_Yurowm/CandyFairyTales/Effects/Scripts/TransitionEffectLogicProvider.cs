using System;
using System.Collections;
using UnityEngine;
using Yurowm;
using Yurowm.ContentManager;
using Yurowm.Effects;
using Yurowm.Extensions;
using Yurowm.Spaces;
using Yurowm.Utilities;

namespace YMatchThree.Core {
    public class TransitionEffectLogicProvider : IEffectLogicProvider {
        public bool IsSuitable(EffectBody effectBody) {
            return effectBody.GetComponent<TransitionEffectInfo>();
        }

        public IEnumerator Logic(LiveContext context, IEffectCallback[] callbacks, Effect effect) {
            if (!effect.body.SetupComponent(out TransitionEffectInfo info))
                yield break;
                
            var callback = callbacks.CastOne<Callback>();

            callback.setupBody?.Invoke(effect.body);

            var startPosition = effect.position;
            var destinationPoint = callback.destinationPoint;
            
            var offset = Vector2.right
                .Rotate(effect.random.Range(info.offsetAngle)) 
                         * effect.random.Range(info.offsetDistace);
            
            var speed = effect.random.Range(info.speed) /
                        ((destinationPoint - startPosition).FastMagnitude() + offset.FastMagnitude());
            
            var time = effect.time;
            
            for (var t = 0f; t < 1; t += time.Delta * speed) {
                var lastPosition = effect.position;
                var easeT = t.Ease(info.easing);
                
                effect.position = Vector2.Lerp(
                    startPosition,
                    destinationPoint + offset * (1f - t),
                    easeT);
                
                effect.size = info.scale.Evaluate(easeT);
                
                if (info.rotate)
                    effect.direction = (effect.position - lastPosition).Angle();
                
                yield return null;
            }
            
            effect.position = destinationPoint;
            effect.size = info.scale.Evaluate(1);
            
            if (effect.effectBody.SetupComponent(out ContentSound sound))
                sound.Play("Hit");
            
            if (effect.effectBody.SetupComponent(out ContentAnimator animator))
                yield return animator.PlayAndWait("Hit");

            callback.onComplete?.Invoke();
        }

        public struct Callback : IEffectCallback {
            public Action<SpaceObject> setupBody;
            public Action onComplete;
            public Vector2 destinationPoint;
        }
    }
}