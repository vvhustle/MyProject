using UnityEngine;

namespace Yurowm {
    [RequireComponent(typeof(ContentAnimator))]
    public class ContentAnimatorAutoplay : BaseBehaviour {
        
        ContentAnimator animator;
        
        public enum Event {
            Awake,
            OnEnable
        }
        
        public WrapMode wrapMode = WrapMode.Default;
        
        public Event eventType;
        
        public string clipName;
        
        public float timeScale = 1;
        public float timeOffset = 0;

        void Awake() {
            animator = GetComponent<ContentAnimator>();
            
            if (eventType == Event.Awake)
                animator.Play(clipName, wrapMode, timeScale, timeOffset);
        }

        void OnEnable() {
            if (eventType == Event.OnEnable)
                animator.Play(clipName, wrapMode, timeScale, timeOffset);
        }
    }
}