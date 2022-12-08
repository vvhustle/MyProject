using System;
using System.Collections;
using TMPro;
using UnityEngine;
using Yurowm.ContentManager;
using Yurowm.Extensions;

namespace Yurowm.Dialogues {
    public class SpeachBubble : ContextedBehaviour, IReserved {
        
        public TMP_Text label;
        public RectTransform arrow;
        public float arrowOffset = 10;

        [NonSerialized]
        public Character character;
        
        ContentAnimator animator;

        public override void Initialize() {
            base.Initialize();
            this.SetupComponent(out animator);
        }
        
        void Update() {
            if (arrow && character?.speachSource != null) {
                Debug.DrawRay(character.speachSource.position, Vector3.up, Color.red);
                Debug.DrawRay(arrow.position, Vector3.up, Color.green);
                Debug.DrawLine(character.speachSource.position, arrow.position, Color.cyan);
                ApplyArrow(character.speachSource.position - arrow.position);
            }
        }

        public void Link(Transform parent) {
            transform.SetParent(parent);
            rectTransform.Maximize();
        }
        
        void ApplyArrow(Vector3 position) {
            if (!arrow && arrow.lossyScale.z > 0) 
                return;
            
            var size = (position.magnitude / arrow.lossyScale.z) - arrowOffset;
            
            if (size > 0.1f) {
                arrow.gameObject.SetActive(true);
                arrow.sizeDelta = arrow.sizeDelta.ChangeX(size);
                arrow.rotation = Quaternion.Euler(0, 0, GetArrowAngle(position));
            } else
                arrow.gameObject.SetActive(false);

        }

        float GetArrowAngle(Vector3 position) {
            return position.To2D().Angle();
        }
        
        public IEnumerator Hide() {
            character = null;
            yield return animator?.PlayAndWait("Hide");
            Kill();
        }

        public IEnumerator Show() {
            yield return animator?.PlayAndWait("Show");
        }
        
        public void Rollout() {
            character = null;
            ApplyArrow(default);
        }
    }
}