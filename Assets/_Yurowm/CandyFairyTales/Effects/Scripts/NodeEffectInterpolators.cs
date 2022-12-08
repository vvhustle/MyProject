
using UnityEngine;
using Yurowm.Effects;
using Yurowm.Extensions;
using Yurowm.Utilities;

namespace YMatchThree.Core {
    public abstract class NodeEffectInterpolator : IEffectCallback {
        public abstract void SetInitialPosition(Vector2 position);
        
        public virtual void OnFrame() {}

        public abstract Vector2 GetNextPosition(Vector2 current, Vector2 target);
    }
    
    public class NodeEffectLinearInterpolator : NodeEffectInterpolator {
        
        FloatRange speedRange;
        
        protected float speed;
        float acceleration;
        
        public NodeEffectLinearInterpolator(FloatRange speedRange, float acceleration) {
            this.speedRange = speedRange;
            speed = speedRange.min;
            this.acceleration = acceleration;
        }
        
        public override void SetInitialPosition(Vector2 position) {}

        public override void OnFrame() {
            speed = speedRange.Clamp(speed + acceleration * Time.deltaTime);
        }

        public override Vector2 GetNextPosition(Vector2 current, Vector2 target) {
            return Vector2.MoveTowards(current, target, speed * Time.deltaTime);
        }
    }
    
    public class NodeEffectCurveInterpolator : NodeEffectInterpolator {
        float time = 1f;
        float duration;
        Vector2 offset;
        
        Vector2 initialPosition;

        public NodeEffectCurveInterpolator(float duration, float offset, float angle) {
            this.duration = duration;
            this.offset = (Vector2.right * offset).Rotate(angle);
        }

        public override void SetInitialPosition(Vector2 position) {
            initialPosition = position;
        }

        public override void OnFrame() {
            time -= Time.deltaTime / duration;
            time = Mathf.Clamp01(time);
        }

        public override Vector2 GetNextPosition(Vector2 current, Vector2 target) {
            return Vector2.Lerp(target + offset * time, initialPosition, time);
        }
    }
}