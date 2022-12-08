using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using YMatchThree.Core;
using Yurowm.ContentManager;
using Yurowm.Core;
using Yurowm.Coroutines;
using Yurowm.Extensions;
using Yurowm.UI;
using Yurowm.Utilities;
using Space = Yurowm.Spaces.Space;

namespace Yurowm.Dialogues {
    public class Dialogue : GameEntity, IDisposable {
        
        Dictionary<Side, Character> characters = new Dictionary<Side, Character>();
        
        SpeachBubble bubble;
        
        FieldRectController fieldRectController;
        
        public enum Side {
            Left = 0,
            Right = 1,
            LeftBack = 2,
            RightBack = 3,
        }

        #region Acting

        bool acting = false;
        
        public bool IsActing => acting;
        
        public Dialogue Act() {
            if (acting)
                throw new Exception("It already acting!");
            acting = true;
            return this;
        }

        public void Dispose() {
            acting = false;
        }
        
        public void Release() {
            if (characters.IsEmpty())
                Kill();
        }

        #endregion

        public IEnumerator Hide(Side side) {
            if (characters.TryGetValue(side, out var character)) {
                if (bubble && bubble.character == character)
                    yield return HideBubble();
                
                if (character.parent.SetupComponent(out ContentAnimator animator))
                    yield return animator.PlayAndWait("Hide");
                
                character.Kill();
                characters.Remove(side);
            }
        }

        public IEnumerator Show(Side side, string characterName) {
            bool shown = false;
            
            var characterAsset = AssetManager.GetPrefab<Character>(characterName);
            
            if (characters.TryGetValue(side, out var character)) {
                if (characterAsset && characterAsset.characterID != character.characterID)
                    yield return Hide(side);
                else { 
                    shown = true;
                    if (bubble && bubble.character == character)
                        bubble.character = null;
                    character.Kill();
                    characters.Remove(side);
                }
            }
            
            if (!characterAsset) yield break;
            
            var parent = GetParent(side);
            
            if (!parent) yield break;
            
            character = AssetManager.Emit(characterAsset);
            
            characters.Add(side, character);
            
            character.Link(parent);
            
            yield return Page.WaitAnimation();

            if (!shown && parent.SetupComponent(out ContentAnimator animator))
                yield return animator.PlayAndWait("Show");
        }
        
        public IEnumerator TapToContinue() {
            var button = Behaviour.GetByID<Button>("Dialogue.TapToContinue");
            
            if (button.SetupComponent(out ContentAnimator animator)) {
                yield return animator.WaitPlaying();
                yield return animator.PlayAndWait("Show");
            }
            
            if (!button)
                yield break;
            
            var clicked = false;
            
            void OnClick() {
                clicked = true;
            }
            
            button.gameObject.SetActive(true);
            button.onClick.AddListener(OnClick);
            
            while (!clicked)
                yield return null;

            button.onClick.RemoveListener(OnClick);
            
            if (animator)
                yield return animator.PlayAndWait("Hide");
            
            button.gameObject.SetActive(false);
        }
        
        public IEnumerator Say(Side side, string text) {
            if (bubble)
                yield return HideBubble();
            
            var parent = GetParent("Bubble");
            
            if (!parent)
                yield break;
            
            bubble = AssetManager.Emit<SpeachBubble>();
            bubble.Link(parent);
            
            bubble.label.text = text;
            
            if (characters.TryGetValue(side, out var character)) 
                bubble.character = character;

            yield return bubble.Show();
        }
        
        public IEnumerator HideBubble() {
            if (bubble)
                yield return bubble.Hide();
        }
        
        Transform GetParent(string name) {
            return GetParents(name).FirstOrDefault(p => p.gameObject.activeInHierarchy);
        }
        
        IEnumerable<Transform> GetParents(string name) {
            return ObjectTag
                .GetAll<Transform>($"Character.{name}");
        }
        
        Transform GetParent(Side side) {
            return GetParent(side.ToString());
        }
        
        IEnumerator AnimateRects(bool state) {
            while (IsActing)
                yield return null;

            using (Act()) {
                fieldRectController.SetMessageState(this, state);

                while (fieldRectController.IsAnimating())
                    yield return null;
            }
        }

        public override void OnAddToSpace(Space space) {
            base.OnAddToSpace(space);
            Enum.GetValues(typeof(Side))
                .Cast<Side>()
                .SelectMany(s => GetParents(s.ToString()))
                .Select(s => s?.GetComponent<ContentAnimator>())
                .NotNull()
                .ForEach(a => a.Rewind("Show"));
            
            context.Catch<FieldRectController>(c => {
                fieldRectController = c;
                AnimateRects(true).Run(space.coroutine);
            });
            
            App.onScreenResize += Refresh;
        }

        public override void OnRemoveFromSpace(Space space) {
            AnimateRects(false).Run(space.coroutine);
            App.onScreenResize -= Refresh;
            base.OnRemoveFromSpace(space);
            characters.Values.ForEach(c => c.Kill());
            characters.Clear();
            bubble?.Kill();
        }

        void Refresh() {
            foreach (var pair in characters) {
                var side = pair.Key;
                var character = pair.Value;
                
                var parent = GetParent(side);
            
                if (parent) {
                    character.Link(parent);
                    if (parent.SetupComponent(out ContentAnimator a))
                        a.RewindEnd("Show");
                }
            }
            if (bubble) {
                var parent = GetParent("Bubble");
                if (parent)
                    bubble.Link(parent);
            }
        }
    }
}