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
    public class FlyEffectLogicProvider : IEffectLogicProvider {
        public bool IsSuitable(EffectBody effectBody) {
            return effectBody.GetComponent<FlyEffectInfo>();
        }

        public IEnumerator Logic(LiveContext context, IEffectCallback[] callbacks, Effect effect) {
            if (!effect.body.SetupComponent(out FlyEffectInfo info))
                yield break;
            
            if (!callbacks.ContainsType(typeof(Callback))) {
                yield break;
            }
            
            var callback = callbacks.CastOne<Callback>();
            var time = effect.time;
            var random = effect.random;
            var targetUpdateAccess = new DelayedAccess(0.2f);
            
            callback.setupBody?.Invoke(effect.body);
            
            var size = 0f;
            
            void UpdateSize(float newSize) {
                size += (newSize.Clamp01() - size) * time.Delta * 4;
                effect.size = info.scale.Evaluate(size);
            }

            var angle = random.Range(0f, 360f);
            var angularDirection = random.Sign();
            
            var velocity = Vector2.right
                .Rotate(angle) *
                info.speed.min;

            var targetPoint = callback.pullTarget();

            void UpdateTarget() {
                if (targetUpdateAccess.GetAccess())
                    targetPoint = callback.pullTarget();
            }
            
            var angularSpeed = 180f;

            void Move() {
                var offset = targetPoint - effect.position;
                
                if (Mathf.DeltaAngle(angle, offset.Angle()).Abs() > 45) {
                    angularSpeed += 90f * time.Delta;
                    
                    angle += angularSpeed * angularDirection * time.Delta;
                    
                    var speed = velocity.FastMagnitude();
                    speed -= info.acceleration * time.Delta;
                    speed = info.speed.Clamp(speed);

                    velocity = Vector2.right.Rotate(angle) * speed;
                    
                    UpdateSize(1f);
                } else {
                    // angularSpeed = 180f;
                    angularDirection = random.Sign();
                    
                    velocity = velocity.MoveTowards(
                        (targetPoint - effect.position).WithMagnitude(info.speed.max), 
                        info.acceleration * time.Delta);
                    angle = velocity.Angle();
                    
                    UpdateSize(0f);
                }
                
                effect.position += velocity * time.Delta;
                
                if (info.rotate)
                    effect.direction = angle;
                
                var c = effect.space.clickables;
                
                Debug.DrawLine(c.GetWorldPoint(effect.position), c.GetWorldPoint(targetPoint), Color.red);
            }
            
            while ((effect.position - targetPoint).MagnitudeIsGreaterThan(callback.accuracy)) {
                UpdateTarget();
                Move();

                yield return null;
            }
            
            var speed = velocity.FastMagnitude();
            
            while (effect.position != targetPoint) {
                velocity = velocity.MoveTowards(Vector2.zero, info.acceleration * time.Delta);
                
                var newPosition = (effect.position + velocity * time.Delta)
                    .MoveTowards(targetPoint, (speed - velocity.FastMagnitude()) * time.Delta);
                        
                angle = Mathf.MoveTowardsAngle(angle.Repeat(360), 
                    (newPosition - effect.position).Angle(), 
                    700f * time.Delta);
                
                if (info.rotate)
                    effect.direction = angle;
                
                effect.position = newPosition;
                    
                UpdateSize(0f);
                
                yield return null;
            }
            
            callback.onComplete?.Invoke();
            
            if (effect.effectBody.SetupComponent(out ContentSound sound))
                sound.Play("Hit");
            
            if (effect.effectBody.SetupComponent(out ContentAnimator animator))
                yield return animator.PlayAndWait("Hit");
        }

        public struct Callback : IEffectCallback {
            public Action<SpaceObject> setupBody;
            public float accuracy;
            public Action onComplete;
            public Func<Vector2> pullTarget;
        }
    }
}