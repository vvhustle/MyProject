using UnityEngine;
using Yurowm.Utilities;

namespace Yurowm.Effects {
    public class FlyEffectInfo : MonoBehaviour {
        public FloatRange speed = 10;
        public float acceleration = 5;
        public AnimationCurve scale = AnimationCurve.Constant(0, 1, 1);
        public bool rotate = false;
    }
}