using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Linq;
using UnityEngine.Events;
using Yurowm;
using Yurowm.DebugTools;
using Yurowm.Extensions;
using Yurowm.Utilities;

namespace YMatchThree.Core {
    [RequireComponent (typeof (RectTransform))]
    public class UIAreaRect : Yurowm.UIBehaviour, IOnAnimateHandler {

        public bool hideImage = true;
        public Vector2 position {get; private set;}
        public Vector2 size {get; private set;}
        public Vector2 screenSize {get; private set;}
        
        public FloatRange camSizeRange = new FloatRange(.1f, 100f);
        
        public UnityEvent onChanged;
        
        public override void OnRegister() {
            base.OnRegister();
            
            if (hideImage && this.SetupComponent(out Graphic graphic))
                graphic.enabled = false;
        }

        void OnEnable() {
            UpdateParameters();
        }

        public void OnAnimate() {
            OnChange();
        }

        #if UNITY_EDITOR
        
        protected override void OnValidate() {
            base.OnValidate();
            if (Application.isPlaying)
                OnChange();
        }
        
        #endif

        public void UpdateParameters() {
            var canvasRect = GetComponentInParent<Canvas>()?.transform.rect();
            
            if (canvasRect) {
                position = canvasRect.InverseTransformPoint(rectTransform.position);
                screenSize = canvasRect.sizeDelta;
            }
            
            size = rectTransform.rect.size;
        }
        
        protected override void OnRectTransformDimensionsChange() {
            base.OnRectTransformDimensionsChange();
            OnChange();
        }
        
        void OnChange() {
            if (isActiveAndEnabled) {
                onChanged?.Invoke();
            }
        }
    }
}