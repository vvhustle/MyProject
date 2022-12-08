using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Yurowm;
using Yurowm.Coroutines;
using Yurowm.Extensions;
using Yurowm.Utilities;

namespace YMatchThree.Core {
    public class LevelContentAnimator {
        
        LevelContent levelContent;
        public ContentAnimator animator;
        ContentSound sound;
        
        public PuzzleSimulation simulation;
        
        List<IAnimationLayer> layers = new List<IAnimationLayer>();
        
        public LevelContentAnimator(LevelContent levelContent) {
            this.levelContent = levelContent;
            
            levelContent.body?.SetupComponent(out animator);
            levelContent.body?.SetupComponent(out sound);
        }
        
        public bool HasClip(string clipName) {
            if (animator && !clipName.IsNullOrEmpty())
                return animator.HasClip(clipName);
            return false;
        }
        
        public void PlaySound(string clipName) {
            if (simulation.AllowSounds() && sound)
                sound.Play(clipName);
        }

        void PlayClipInternal(string clipName, WrapMode wrapMode) {
            PlaySound(clipName);
            if (simulation.AllowAnimations() && animator)
                animator.Play(clipName, wrapMode);
        }

        public OneShotLayer PlayClip(string clipName) {
            PlaySound(clipName);
            
            if (!simulation.AllowAnimations() || !animator)
                return null;
            
            if (currentlayer is OneShotLayer l && l.clip == clipName)
                return l;
            
            var layer = new OneShotLayer();
            
            layer.clip = clipName;
            
            layers.Add(layer);
            layers.Sort(c => c.priority);
            
            Animate();
            
            return layer;
        }

        public IEnumerator PlayClipAndWait(string clipName) {
            var layer = PlayClip(clipName);
            
            if (layer == null)
                yield break;

            while (!layer.isComplete)
                yield return null;
        }

        public IEnumerator Wait() {
            while (currentlayer != null && !currentlayer.isComplete)
                yield return null;
        }

        IEnumerator PlayClipAndWaitInternal(string clipName) {
            if (simulation.AllowAnimations() && animator)
                yield return animator.PlayAndWait(clipName);
        }
        
        public OpenLoopCloseLayer PlayOpenLoopClose(string openClip, string loopClip, string closeClip, int priority) {
            if (!simulation.AllowAnimations() || !animator)
                return null;
            
            var layer = new OpenLoopCloseLayer();
            
            layer.open = openClip;
            layer.loop = loopClip;
            layer.close = closeClip;
            layer.priority = priority;
            
            layers.Add(layer);
            layers.Sort(c => c.priority);
            
            Animate();
            
            return layer;
        }
        
        public void Stop(IAnimationLayer layer) {
            if (layer == null || !layers.Contains(layer)) 
                return;
            
            layers.Remove(layer);
            layer.Stop();
            isDirty = true;
        }
        
        public IEnumerator StopAndWait(IAnimationLayer layer) {
            if (layer == null || !layers.Contains(layer)) yield break;
            
            Stop(layer);
            
            if (layer == currentlayer)
                yield return Wait();
        }

        public IEnumerator StopAllAndWait() {
            layers.ForEach(l => l.Stop());
            layers.Clear();

            isDirty = true;

            while (currentlayer != null && !currentlayer.isComplete)
                yield return null;
        }
        
        public void StopAllImmediate() {
            layers.ForEach(l => l.Stop());
            layers.Clear();
            
            animator?.Stop();
            
            if (logic == null)
                return;
            
            logic.Stop(levelContent.field.coroutine);
            logic = null;
        }

        void Animate() {
            isDirty = true;
            if (logic == null) {
                logic = Logic();
                logic.Run(levelContent.field.coroutine);
            }
        }

        public bool IsPlaying() {
            return !layers.IsEmpty();
        }
        
        bool isDirty = false;
        
        IEnumerator logic;
        IAnimationLayer currentlayer;
        
        IEnumerator Logic() {
            var breaker = false;
            
            Func<bool> Breaker = () => breaker;
            
            var coroutine = levelContent.field?.coroutine;
            
            while (!layers.IsEmpty()) {
                
                if (animator && animator.IsPlaying()) {
                    animator.Complete();
                    yield return animator.WaitPlaying();
                }
                
                currentlayer = layers.FirstOrDefault();
                
                if (currentlayer == null)
                    continue;
                
                if (!currentlayer.isComplete) {
                    var complete = false;
                    
                    breaker = false;
                    
                    currentlayer.Play(this, Breaker)
                        .ContinueWith(() => complete = true)
                        .Run(coroutine);

                    while (!complete) {
                        
                        if (isDirty) {
                            if (currentlayer != layers.FirstOrDefault())
                                breaker = true;
                        }
                        
                        yield return null;
                    }
                    
                    isDirty = false;
                }

                if (currentlayer.isComplete)
                    layers.Remove(currentlayer);
            }
            
            logic = null;
        }

        public interface IAnimationLayer {
            int priority { get; }
            bool isComplete { get; }
            
            IEnumerator Play(LevelContentAnimator lca, Func<bool> isBroken);
            void Stop();
        }
        
        public class OneShotLayer : IAnimationLayer {
            internal string clip; 
            
            public bool isComplete {get; private set; } = false;
            
            public int priority => 999;
            
            public IEnumerator Play(LevelContentAnimator lca, Func<bool> isBroken) {
                if (!clip.IsNullOrEmpty()) {
                    lca.PlaySound(clip);
                    yield return lca.animator.PlayAndWait(clip);
                }
                
                isComplete = true;
            }

            public void Stop() { }
        }
        
        public class OpenLoopCloseLayer : IAnimationLayer {
            internal string open; 
            internal string loop;
            internal string close;
            
            internal string UID = YRandom.main.GenerateKey(16);
            
            public bool isComplete {get; private set; } = false;
            
            public int priority {get; set;} = 0;
            
            public IEnumerator Play(LevelContentAnimator lca, Func<bool> isBroken) {
                if (lca.HasClip(open)) {
                    lca.PlaySound(open);
                    yield return lca.PlayClipAndWaitInternal(open);
                }

                if (lca.HasClip(loop)) {
                    lca.PlayClipInternal(loop, WrapMode.Loop);
                    while (lca.animator.IsPlaying(loop) && !isBroken() && !toComplete)
                        yield return null;
                    yield return lca.animator.CompleteAndWait(loop);
                } else
                    while (!isBroken() && !toComplete)
                        yield return null;

                if (lca.HasClip(close)) {
                    lca.PlaySound(close);
                    yield return lca.PlayClipAndWaitInternal(close);
                }
                
                if (toComplete)
                    isComplete = true;
            }
            
            bool toComplete = false;
            
            public void Stop() {
                toComplete = true;
            }
        }
    }
}