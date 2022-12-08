using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Yurowm.Extensions;

namespace Yurowm.UI {
    public class UIOrientationRect : UIOrientationBase {
        
        [HideInInspector]
        public List<Rect> rects = new List<Rect>();

        public override void OnScreenResize() {
            if (Application.isPlaying && !isActiveAndEnabled) 
                return;
            
            var currentOrientation = GetCurrentOrientation();
            
            rects
                .FirstOrDefaultFiltered(
                    r => r.orientation.HasFlag(currentOrientation),
                    r => r.orientation.OverlapFlag(currentOrientation))?
                .Apply(rectTransform);
        }

        [Serializable]
        public class Rect {
            public Orientation orientation = Orientation.Landscape;
            public Vector2 pivot;
            public Vector2 anchorMin;
            public Vector2 anchorMax;
            public Vector2 offsetMin;
            public Vector2 offsetMax;
            public Quaternion rotation;
            public Vector3 localScale;
            
            public void Apply(RectTransform rectTransform) {
                rectTransform.pivot = pivot;
            
                rectTransform.anchorMin = anchorMin;
                rectTransform.anchorMax = anchorMax;
            
                rectTransform.offsetMin = offsetMin;
                rectTransform.offsetMax = offsetMax;
            
                rectTransform.rotation = rotation;
                rectTransform.localScale = localScale;
            }
            
            public void Write(RectTransform rectTransform) {
                pivot = rectTransform.pivot;
            
                anchorMin = rectTransform.anchorMin;
                anchorMax = rectTransform.anchorMax;
            
                offsetMin = rectTransform.offsetMin;
                offsetMax = rectTransform.offsetMax;
            
                rotation = rectTransform.rotation;
                localScale = rectTransform.localScale;
            }
        }
    }
}