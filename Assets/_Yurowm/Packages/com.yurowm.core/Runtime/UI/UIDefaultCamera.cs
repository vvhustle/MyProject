using UnityEngine;

namespace Yurowm.UI {
    [RequireComponent(typeof(Camera))]
    public class UIDefaultCamera : BaseBehaviour {
        void Awake() {
            SetUICamera.SetDefault(GetComponent<Camera>());    
        }
    }
}