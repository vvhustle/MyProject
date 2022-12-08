using UnityEngine;
using Yurowm.Utilities;

namespace Yurowm.Effects {
    public class TransitionEffectInfo : MonoBehaviour {
        public FloatRange speed;
        public FloatRange offsetDistace;
        public FloatRange offsetAngle;
        public EasingFunctions.Easing easing = EasingFunctions.Easing.Linear;
        public AnimationCurve scale = AnimationCurve.Constant(0, 1, 1);
        public bool rotate = false;
    }
}