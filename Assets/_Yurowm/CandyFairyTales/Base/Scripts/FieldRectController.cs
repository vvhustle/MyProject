using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Yurowm;
using Yurowm.Coroutines;
using Yurowm.Dialogues;
using Yurowm.Extensions;
using Yurowm.UI;
using Behaviour = Yurowm.Behaviour;
using Space = Yurowm.Spaces.Space;

namespace YMatchThree.Core {
    public class FieldRectController : GameEntity {
        
        float currentRectTime = 1;
        
        List<object> messages = new List<object>();
        
        public override void OnAddToSpace(Space space) {
            base.OnAddToSpace(space);
            
            Refresh();
        }

        IEnumerator animation;
        
        void Refresh() {
            if (animation != null) return;
            
            animation = AnimateRects();
            animation.Run(space.coroutine);
        }

        public void SetMessageState(object message, bool state) {
            if (messages.Contains(message) == state)
                return;
            
            if (state)
                messages.Add(message);
            else
                messages.Remove(message);
                
            Refresh();
        }

        public bool IsAnimating() => animation != null;

        IEnumerator AnimateRects() {
            yield return Page.WaitAnimation();

            var rects = GetRects();

            using (Page.NewActiveAnimation())
                while (true) {
                    yield return null;

                    var targetTime = messages.IsEmpty() ? 0 : 1;
                    
                    rects.ForEach(r => {
                        currentRectTime = currentRectTime.MoveTowards(targetTime, time.RealDelta * 3);
                        r.Time = currentRectTime;
                    });
                    
                    if (rects.All(r => r.Time == targetTime))
                        break;
                }

            animation = null;
        }
        
        List<RectTransformLerp> GetRects() {
            return Behaviour.GetAllByID<UIAreaRect>("FieldRect")
                .Select(r => r.GetComponent<RectTransformLerp>())
                .NotNull()
                .ToList();
        }
    }
}