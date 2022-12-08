using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Yurowm.Analytics;
using Yurowm.Coroutines;
using Yurowm.Extensions;
using Yurowm.UI;

namespace Yurowm {
    public class Button : UIBehaviour, IPointerUpHandler, IPointerDownHandler {
        public UnityEvent onClick;

        ContentAnimator animator;
        ContentSound sound;
        public string eventName;
        
        [SerializeField]
        bool m_Interactable = true;
        public bool interactable {
            get => m_Interactable;
            set {
                if (m_Interactable == value) return;
                
                m_Interactable = value;
                
                animator?.Rewind(m_Interactable ? "Unlock" : "Lock");
            }
        }

        public override void Initialize() {
            base.Initialize();
            
            gameObject.SetupComponent(out animator);
            gameObject.SetupComponent(out sound);
        }

        protected override void OnEnable() {
            base.OnEnable();
            animator?.Play("Awake");
        }
        
        public void SetAction(Action action) {
            NoAction();
            onClick.AddListener(action.Invoke);
        }

        public void NoAction() {
            onClick.RemoveAllListeners();
        }

        void ResetAnimation() {
            animator?.Rewind("Awake");
        }

        enum State {
            None,
            PressDown,
            PressUp,
            Escaped
        }
        
        State state = State.None;
        
        IEnumerator pressing = null;

        const string pressDownClip = "PressDown";
        const string successClip = "Click";
        
        IEnumerator Pressing() {
            state = State.PressDown;

            sound?.Play(pressDownClip);
            animator?.Play(pressDownClip);

            while (state == State.PressDown)
                yield return null;
            
            if (state == State.PressUp) {
                
                sound?.Play(successClip);
                animator?.Play(successClip);
                
                if (!eventName.IsNullOrEmpty())
                    Analytic.Event($"ButtonPress_{eventName}");
                
                try {
                    onClick.Invoke();
                } catch (Exception e) {
                    Debug.LogException(e);
                }
            }
            
            if (animator) {
                while (animator.IsPlaying())
                    yield return null;
            }

            state = State.None;
            pressing = null;
        }
        
        #region Pointer Handlers

        public void OnPointerUp(PointerEventData eventData) {
            if (!interactable || pressing == null) return;
            
            if (!eventData.dragging && eventData.pointerCurrentRaycast.gameObject == gameObject
                && InputLock.GetAccess(contextTag.ID)) {
                state = State.PressUp;
            } else {
                state = State.Escaped;
            }
        }

        public void OnPointerDown(PointerEventData eventData) {
            if (!interactable || pressing != null) 
                return;
            
            if (!InputLock.GetAccess(contextTag.ID)) 
                return;

            pressing = Pressing();
            pressing.Run();
        }

        #endregion
    }
}