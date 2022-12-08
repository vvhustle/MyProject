using System.Collections.Generic;
using UnityEngine;
using Yurowm.Core;
using Yurowm.Utilities;

namespace Yurowm.UI {
    [RequireComponent(typeof(RectTransform))]
    public class UISafeArea : MonoBehaviour {
        void OnDisable() {
            App.onScreenResize -= ApplySafeArea;
        }

        void OnEnable() {
            App.onScreenResize += ApplySafeArea;    
            ApplySafeArea();    
        }

        void ApplySafeArea() {
            var safeArea = App.safeOffset;
            var full = new Vector2(Screen.width, Screen.height);
            
            var rect = transform as RectTransform;

            var min = new Vector2(
                safeArea.Left / full.x,
                safeArea.Bottom / full.y);
            var max = new Vector2(
                1f - safeArea.Right / full.x, 
                1f - safeArea.Top / full.y);
            
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = default;
            rect.offsetMax = default;
        }
    }
}