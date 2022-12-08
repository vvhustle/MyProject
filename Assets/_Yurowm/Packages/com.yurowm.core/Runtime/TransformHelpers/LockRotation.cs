using UnityEngine;

namespace Yurowm {
    public class LockRotation : BaseBehaviour {
        void Update() {
            transform.rotation = Quaternion.identity;
        }
    }
}