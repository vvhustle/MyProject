using System;
using UnityEngine;
using Yurowm.Core;
using Yurowm.Utilities;

namespace Yurowm.UI {
    public abstract class UIOrientationBase : Behaviour {
        [Flags]
        public enum Orientation {
            Landscape = 1 << 1,
            Portrait = 1 << 2,
            Phone = 1 << 3,
            Tablet = 1 << 4
        }

        public override void Initialize() {
            base.Initialize();
            App.onScreenResize += OnScreenResize;
        }

        public override void OnKill() {
            base.OnKill();
            App.onScreenResize -= OnScreenResize;
        }

        void OnEnable() {
            OnScreenResize();
        }

        public static Orientation GetCurrentOrientation() {
            Orientation result = 0;
            
            var aspectRatio = 1f * Screen.width / Screen.height;
            
            #if UNITY_EDITOR
            if (!Application.isPlaying && Camera.main != null) 
                aspectRatio = Camera.main.aspect;
            #endif
            
            if (aspectRatio >= 1)
                result |= Orientation.Landscape;
            else
                result |= Orientation.Portrait;
            
            if (App.isTablet)
                result |= Orientation.Tablet;
            else
                result |= Orientation.Phone;
            
            return result;
        }

        public abstract void OnScreenResize();
    }
}