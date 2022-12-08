using System;
using System.Collections;
using Yurowm.ContentManager;
using Yurowm.Extensions;

namespace Yurowm.Effects {
    [Flags]
    public enum EffectType {
        Particles = 1 << 0,
        Shards = 1 << 2,
        Node = 1 << 3,
        SlotHighlighter = 1 << 4,
        Lined = 1 << 5
    }
    
    public interface IEffectCallback {}
    
    public struct CompleteCallback : IEffectCallback {
        public Action onComplete;
    }
    
    [JITMethodIssueType]
    public interface IEffectLogicProvider {
        bool IsSuitable(EffectBody effectBody);
        IEnumerator Logic(LiveContext context, IEffectCallback[] callbacks, Effect effect);
    }
    
    public class AnimationEffectLogicProvider : IEffectLogicProvider {
        const string awakeClip = "Start";
        
        public bool IsSuitable(EffectBody effectBody) {
            return effectBody.GetComponent<ContentAnimator>()?.HasClip(awakeClip) ?? false;
        }

        public IEnumerator Logic(LiveContext context, IEffectCallback[] callbacks, Effect effect) {
            if (effect.effectBody.SetupComponent(out ContentAnimator animator))
                yield return animator.PlayAndWait(awakeClip);
        }
    }
    
    public class SoundEffectLogicProvider : IEffectLogicProvider {
        const string awakeClip = "Start";
        
        public bool IsSuitable(EffectBody effectBody) {
            return effectBody.GetComponent<ContentSound>();
        }

        public IEnumerator Logic(LiveContext context, IEffectCallback[] callbacks, Effect effect) {
            if (effect.effectBody.SetupComponent(out ContentSound sound))
                sound.Play(awakeClip);
            yield break;
        }
    }
    
}