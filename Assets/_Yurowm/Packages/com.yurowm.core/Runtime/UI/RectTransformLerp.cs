using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yurowm.Extensions;
using Yurowm.Utilities;

namespace Yurowm.UI {
    [RequireComponent(typeof(RectTransform))]
    [ExecuteAlways]
    public class RectTransformLerp : UnityEngine.EventSystems.UIBehaviour {
        
        public RectTransform rectStart;
        public RectTransform rectEnd;
        
        RectTransform _rectTransform;
        RectTransform rectTransform {
            get {
                if (!_rectTransform)
                    this.SetupComponent(out _rectTransform);
                return _rectTransform;
            }   
        }

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
        
        public EasingFunctions.Easing easing = EasingFunctions.Easing.Linear;

        #if UNITY_EDITOR
        protected override void OnValidate() {
            base.OnValidate();
            Refresh();    
        }
        #endif

        protected override void OnEnable() {
            base.OnEnable();
            Refresh();
        }

        protected override void OnDidApplyAnimationProperties() {
            base.OnDidApplyAnimationProperties();
            Refresh();    
        }

        void Refresh() {
            if (!isActiveAndEnabled) return;
            
            var t = easing.Evaluate(time.Clamp01());
            
            if (!rectStart || !rectEnd) return;
            if (rectStart.parent != rectEnd.parent) return;
            if (rectTransform.parent != rectStart.parent) return;
            
            rectTransform.pivot = Vector2.Lerp(rectStart.pivot, rectEnd.pivot, t);
            
            rectTransform.anchorMin = Vector2.Lerp(rectStart.anchorMin, rectEnd.anchorMin, t);
            rectTransform.anchorMax = Vector2.Lerp(rectStart.anchorMax, rectEnd.anchorMax, t);
            
            rectTransform.offsetMin = Vector2.Lerp(rectStart.offsetMin, rectEnd.offsetMin, t);
            rectTransform.offsetMax = Vector2.Lerp(rectStart.offsetMax, rectEnd.offsetMax, t);
            
            rectTransform.rotation = Quaternion.Lerp(rectStart.rotation, rectEnd.rotation, t);
            rectTransform.localScale = Vector3.Lerp(rectStart.localScale, rectEnd.localScale, t);
        }
    }
}