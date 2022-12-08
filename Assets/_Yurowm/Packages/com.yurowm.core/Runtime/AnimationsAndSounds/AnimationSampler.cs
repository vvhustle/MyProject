using UnityEngine;

namespace Yurowm {
    [ExecuteAlways]
    public class AnimationSampler : BaseBehaviour {

        public AnimationClip clip;
        
        [SerializeField]
        [Range(0, 1)]
        float time = 0;
        
        public float Time {
            get => time;
            set {
                value = value.Clamp01();
                if (time == value) return;
                time = value;
                Refresh();
            }
        }
        
        public float RealTime {
            get => Time * Length;
            set => Time = value / Length;
        }

        public float Length => clip?.length ?? 0;

        public void Zero() {
            Time = 0;            
        }

        void OnValidate() {
            Refresh();    
        }
        
        void OnDidApplyAnimationProperties() {
            Refresh();    
        }

        void Refresh() {
            clip?.SampleAnimation(gameObject, time.Clamp(0, 0.9999f) * clip.length);
        }
    }
}
