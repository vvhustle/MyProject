using UnityEngine;
using Yurowm.ContentManager;

namespace Yurowm {
    public abstract class UIBehaviour : UnityEngine.EventSystems.UIBehaviour, IBehaviour {
        public LiveContext context { get; set; }

        public ContextTag ContextTag => contextTag;
        public ContextTag contextTag;
        
        public bool visible => isActiveAndEnabled;
        
        Transform _transform;
        public new Transform transform {
            get {
                if (!_transform)
                    _transform = base.transform;
                return _transform;
            }
        }

        public RectTransform rectTransform => transform as RectTransform;

        protected override void Awake() {
            base.Awake();
            Behaviour.Register(this);    
        }

        protected override void OnDestroy() {
            base.OnDestroy();
            if (!killed) OnKill();
        }

        public virtual void OnRegister() {}
        public virtual void Initialize() {}

        public virtual void OnKill() {
            if (killed) return;
            
            killed = true;
            
            Behaviour.Unregister(this);
        }
        
        #region ILiveContexted
        
        bool killed = false;
        
        public void Kill() {
            if (!killed) OnKill();
            Destroy(this);
        }

        public bool EqualContent(ILiveContexted obj) {
            return Equals(obj);
        }
        
        #endregion
    }
}