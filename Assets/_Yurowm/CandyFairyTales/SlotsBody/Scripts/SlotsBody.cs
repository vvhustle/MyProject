using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Yurowm;
using Yurowm.ContentManager;
using Yurowm.Coroutines;
using Yurowm.Extensions;
using Yurowm.Spaces;
using Yurowm.Utilities;
using Space = Yurowm.Spaces.Space;

namespace YMatchThree.Core {
    public class SlotsBody : SpaceObject, IReserved {
        public SlotRenderer slotRenderer;
        public SpriteRenderer slotBackground;
        
        ContentAnimator animator;
        
        bool animateSlots = false;
        
        bool animate = false;
        
        public Color oddSlotColor = new Color(0.23f, 0.38f, 1f);
        public Color evenSlotColor = new Color(0.23f, 0.32f, 0.55f);

        List<SpriteRenderer> slots = new List<SpriteRenderer>();
        
        const string showClip = "Show";
        const string hideClip = "Hide";

        public override void Initialize() {
            base.Initialize();
         
            animator = GetComponent<ContentAnimator>();
            
            animateSlots = slotBackground 
                           && slotBackground.SetupComponent<ContentAnimator>(out var sanimator)
                           && sanimator.HasClip(showClip);
        }

        int2[] rebuildRequest = null;
        
        public void Rebuild(int2[] points) {
            if (animate) {
                rebuildRequest = points;
                return;
            }
            
            rebuildRequest = null;
            
            slotRenderer?.RebuildImmediate(points);

            if (!slotBackground) return;
            
            ClearSlots();
                
            for (int i = 0; i < points.Length; i++) {
                var slot = EmitSlot(i);       
                var point = points[i];
                slot.color = (point.X + point.Y) % 2 == 0 ? evenSlotColor : oddSlotColor;
                slot.transform.localPosition = (point.ToVector2() + Vector2.one / 2) * Slot.Offset;
            }
        }
        
        public IEnumerator Show() {
            if (animate)
                yield break;

            animate = true;
            
            AnimateSlots(a => a.Rewind(showClip));
            
            if (animator)
                yield return animator.PlayAndWait(showClip);

            yield return PlaySlots(showClip);
            
            animate = false;

            if (rebuildRequest != null)
                Rebuild(rebuildRequest);
        }

        public IEnumerator Hide() {
            if (animate)
                yield break;

            animate = true;

            yield return PlaySlots(hideClip);

            if (animator)
                yield return animator.PlayAndWait(hideClip);
            
            animate = false;
        }

        IEnumerator PlaySlots(string clip) {
            if (!animateSlots) yield break;
            
            ContentAnimator ca = null;
            
            AnimateSlots(a => {
                ca = a;
                ca.Play(clip);
            });
            
            if (ca)
                yield return ca.WaitPlaying();
        }

        void AnimateSlots(Action<ContentAnimator> action) {
            slots
                .Where(s => s.gameObject.activeSelf)
                .ForEach(s => {
                    s.SetupComponent(out ContentAnimator sa);
                    action.Invoke(sa);
                });
        }

        void ClearSlots() {
            slots.ForEach(s => s.gameObject.SetActive(false));
        }
        
        SpriteRenderer EmitSlot(int index) {
            SpriteRenderer result;
            
            if (slots.IsEmpty()) 
                slots.Add(slotBackground);

            if (index < slots.Count)
                result = slots[index];
            else {
                result = Instantiate(slotBackground.gameObject)
                    .GetComponent<SpriteRenderer>();
                
                result.name = $"{slotBackground.name}_{index}";
                result.transform.SetParent(slotBackground.transform.parent); 
                slots.Add(result);
            }
            
            result.gameObject.SetActive(true);
            result.transform.Reset();

            return result;
        }
        
        public void Rollout() {
            slots
                .ForEach(s => {
                    s.SetupComponent(out ContentAnimator sa);
                    sa.Stop();
                });
            rebuildRequest = null;
            Rebuild(new int2[0]);
        }
    }
}