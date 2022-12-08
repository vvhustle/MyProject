using System.Linq;
using UnityEngine;
using Yurowm.ContentManager;
using Yurowm.Extensions;
using Yurowm.Utilities;

namespace Yurowm.UI {
    [RequireComponent(typeof(Canvas))]
    public class SetUICamera : Behaviour {
        
        public float planeDistance = 0;

        [OnLaunch(INITIALIZATION_ORDER + 1)]
        static void OnLaunch() {
            if (!defaultCamera) {
                defaultCamera = AssetManager.Create<Camera>(); 
                if (defaultCamera) {
                    defaultCamera.name = "DefaultUICamera";
                    Set(defaultCamera);
                }
            }
        }

        public override void Initialize() {
            base.Initialize();
            if (currentCamera)
                Set(currentCamera);
        }

        static Camera defaultCamera;
        static Camera currentCamera;
        
        public static void SetDefault(Camera cam) {
            defaultCamera = cam;
            Set(currentCamera);
        }
        
        public static void Set(Camera cam) {
            currentCamera = cam;
            GetAll<SetUICamera>()
                .ForEach(s => {
                    if (s.SetupComponent(out Canvas canvas)) {
                        canvas.worldCamera = cam ?? defaultCamera;
                        canvas.planeDistance = s.planeDistance;
                    }
                });
            defaultCamera?.gameObject.SetActive(cam == null || cam == defaultCamera);
        }
        
        public static void Remove(Camera cam) {
            if (cam == null) return;
            
            bool defaultIsUsed = false;
            
            GetAll<SetUICamera>()
                .Select(s => s.GetComponent<Canvas>())
                .ForEach(c => {
                    if (!c) return;
                    if (c.worldCamera == cam)
                        c.worldCamera = defaultCamera;
                    if (!defaultIsUsed && c.worldCamera == defaultCamera)
                        defaultIsUsed = true;
                });
            defaultCamera?.gameObject.SetActive(defaultIsUsed);
        }
    }
}