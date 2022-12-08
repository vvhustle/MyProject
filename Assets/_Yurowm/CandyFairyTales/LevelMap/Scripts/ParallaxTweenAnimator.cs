using System;
using UnityEngine;
using System.Collections;
using Yurowm;
using Yurowm.ContentManager;
using Yurowm.Spaces;
using Yurowm.Utilities;

namespace YMatchThree.Seasons {
    public class ParallaxTweenAnimator : BaseBehaviour, ILevelMapLocationComponent {

        #region ContextMenu
        
        [UnityEngine.ContextMenu ("Inverse")]
        public void Inverse() {
            var z = bottomPosition;
            bottomPosition = topPosition;
            topPosition = z;

            z = bottomScale;
            bottomScale = topScale;
            topScale = z;

            z.x = bottomRotation;
            bottomRotation = topRotation;
            topRotation = z.x;
        }

        [UnityEngine.ContextMenu("Apply Top")]
        public void ApplyTop() {
            if (position)
                transform.localPosition = topPosition;
            if (rotation)
                transform.rotation = Quaternion.Euler(0, 0, topRotation);
            if (scale)
                transform.localScale = topScale;
        }

        [UnityEngine.ContextMenu("Apply Bottom")]
        public void ApplyBottom() {
            if (position)
                transform.localPosition = bottomPosition;
            if (rotation)
                transform.rotation = Quaternion.Euler(0, 0, bottomRotation);
            if (scale)
                transform.localScale = bottomScale;
        }

        [UnityEngine.ContextMenu("Set Top From Current")]
        public void SetTopFromCurrent() {
            if (position)
                topPosition = transform.localPosition;
            if (rotation)
                topRotation = transform.localEulerAngles.z;
            if (scale)
                topScale = transform.localScale;
        }

        [UnityEngine.ContextMenu("Set Bottom From Current")]
        public void SetBottomFromCurrent() {
            if (position)
                bottomPosition = transform.localPosition;
            if (rotation)
                bottomRotation = transform.localEulerAngles.z;
            if (scale)
                bottomScale = transform.localScale;
        }

        #endregion
        
        SpaceCamera spaceCamera;

        public Transform pivot;

        #if UNITY_EDITOR
        public float value;
        #endif

        public float top = 1f;
        public float bottom = 0f;
        public bool clamped = true;

        public EasingFunctions.Easing easing = EasingFunctions.Easing.Linear;

        public bool position = false;
        public Vector3 topPosition;
        public Vector3 bottomPosition;

        public bool rotation = false;
        public float topRotation;
        public float bottomRotation;

        public bool scale = false;
        public Vector3 topScale = Vector3.one;
        public Vector3 bottomScale = Vector3.one;

        public bool color = false;
        public Color topColor;
        public Color bottomColor;

        void OnEnable() {
            if (spaceCamera)
                spaceCamera.onMove += OnCameraMove;
        }

        void OnDisable() {
            if (spaceCamera)
                spaceCamera.onMove -= OnCameraMove;
        }

        void OnCameraMove(Vector2 _) {
            var pivot = this.pivot ? this.pivot.transform : transform;

            var time = spaceCamera.camera.WorldToViewportPoint(pivot.position).y;

            #if UNITY_EDITOR
            value = time;
            #endif

            time = Mathf.InverseLerp(bottom, top, time);
            
            time = easing.Evaluate(time);
            
            if (easing == EasingFunctions.Easing.Linear && clamped)
                time = time.Clamp01();

            if (position) transform.localPosition = Vector3.LerpUnclamped(bottomPosition, topPosition, time);
            if (rotation) transform.localEulerAngles = Vector3.forward * Mathf.LerpAngle(bottomRotation, topRotation, time);
            if (scale) transform.localScale = Vector3.LerpUnclamped(bottomScale, topScale, time);
            if (color && spriteRenderer) spriteRenderer.color = Color.Lerp(bottomColor, topColor, time);
        }

        void ILevelMapLocationComponent.Initialize(LiveContext context) {
            spaceCamera = context.Get<SpaceCamera>();
            OnEnable();
        }
    }
}