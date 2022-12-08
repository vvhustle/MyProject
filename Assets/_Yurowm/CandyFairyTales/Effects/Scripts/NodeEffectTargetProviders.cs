using System;
using UnityEngine;
using Yurowm.Effects;
using Yurowm.Extensions;
using Yurowm.Spaces;

namespace YMatchThree.Core {
    public abstract class EffectTargetProvider : IEffectCallback {
        public abstract Vector2 GetPosition();

        public abstract bool HasTarget();
        
        public Action onReachTarget;
    }
    
    public class EffectVectorProvider : EffectTargetProvider {
        
        public EffectVectorProvider(Vector2 position = default) {
            this.position = position;
        }
        
        public Vector2 position;
        
        public override Vector2 GetPosition() {
            return position;
        }

        public override bool HasTarget() {
            return true;
        }
    }
    
    public class EffectTransformProvider : EffectTargetProvider {
        
        Transform transform;
        Func<Transform> getNewTarget;

        public EffectTransformProvider(Transform transform, Func<Transform> getNewTarget = null) {
            this.transform = transform;
            this.getNewTarget = getNewTarget;
        }
        
        public override Vector2 GetPosition() {
            return transform?.position.To2D() ?? Vector2.zero;
        }

        public override bool HasTarget() {
            if (transform)
                return true;
            
            transform = getNewTarget?.Invoke();
            
            return transform;
        }
    }
    
    public class NodeEffectItemProvider : EffectTargetProvider {
        SpacePhysicalItem item;
        Func<SpacePhysicalItem> getNewTarget;

        public NodeEffectItemProvider(SpacePhysicalItem item, Func<SpacePhysicalItem> getNewTarget = null) {
            this.item = item;
            this.getNewTarget = getNewTarget;
        }
        
        public override Vector2 GetPosition() {
            if (item != null && item.IsAlive())
                return item.position;
            
            return Vector2.zero;
        }

        public override bool HasTarget() {
            if (item != null && item.IsAlive())
                return true;
            
            item = getNewTarget?.Invoke();
            
            return item != null && item.IsAlive();
        }
    }
}