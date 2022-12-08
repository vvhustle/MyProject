using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Yurowm;
using Yurowm.Colors;
using Yurowm.ContentManager;
using Yurowm.Coroutines;
using Yurowm.Extensions;
using Behaviour = Yurowm.Behaviour;

namespace YMatchThree.Core {
    public class LevelGoalCounter : Behaviour {
        
        public Image[] iconRenderers;
        
        public TextMeshProUGUI label;

        ContentAnimator animator;
        
        public const string completeClip = "Complete";
        public const string awakeClip = "Awake";
        public const string hideClip = "Hide";

        protected override void Awake() {
            base.Awake();
            this.SetupComponent(out animator);
        }

        public void Rollout() {
            label.text = "";
            animator?.Rewind(completeClip);
        }

        public void SetIcon(Sprite sprite) {
            iconRenderers.ForEach(ir => ir.sprite = sprite);
        }

        public void Hide() {
            if (animator)
                animator.PlayAndWait(hideClip)
                    .ContinueWith(() => gameObject.SetActive(false))
                    .Run();
            else
                gameObject.SetActive(false);
        }

        public void Show() {
            Rollout();
            gameObject.SetActive(true);
            animator?.Play(awakeClip);
        }
    }
}