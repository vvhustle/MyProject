using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using YMatchThree.Core;
using Yurowm.ContentManager;
using Yurowm.Coroutines;
using Yurowm.Extensions;
using Yurowm.Utilities;

namespace Yurowm.Effects {
    public class LinedExplosionLogicProvider : IEffectLogicProvider {

        public bool IsSuitable(EffectBody effectBody) {
            return effectBody.GetComponent<LinedExplosionEffect>();
        }

        public IEnumerator Logic(LiveContext context, IEffectCallback[] callbacks, Effect effect) {
            if (!effect.body.SetupComponent(out LinedExplosionEffect lee) || !lee.rightWave)
                yield break;
            
            var callback = callbacks.CastOne<Callback>();
            
            while (callback.lines.IsEmpty())
                yield return null;

            lee.Clear();

            int index = 0;
            
            ContentAnimator wave = null;
            
            List<IEnumerator> animations = new List<IEnumerator>();
            
            foreach (var line in callback.lines) {
                wave = lee.GetWave(index);
                
                wave.transform.localPosition = line.offset.ToVector2() * Slot.Offset;
                
                wave.transform.rotation = Quaternion.Euler(0, 0,
                    line.side.ToAngle());
                
                wave.transform.localScale = new Vector3(
                    line.side.IsStraight() ? 1 : 1f / YMath.SinDeg(45),
                    1, 1);
                
                if (wave.SetupComponent(out SlotRenderer renderer))
                    animations.Add(Animate(renderer, line.size.Clamp(1, Level.maxSize)));

                index++;
            }
            
            yield return animations.Parallel();
            yield return wave?.WaitPlaying();
            
            lee.Clear();
        }
        
        static readonly int _MainTex = Shader.PropertyToID("_MainTex");

        IEnumerator Animate(SlotRenderer renderer, int size) {
            renderer.gameObject.SetActive(true);
            
            var material = renderer.InstanceMaterial;
            
            var waveSize = 1f / material.GetTextureScale(_MainTex).x.ClampMin(.01f);
            
            var offset = material.GetTextureOffset(_MainTex);
            
            if (renderer.SetupComponent(out ContentAnimator animator))
                animator.Play("Awake");
            
            renderer.RebuildImmediate(Enumerator
                .For(0, size, 1)
                .Select(x => new int2(x)));
            
            
            for (float t = 0f; t < size + waveSize; t += Time.unscaledDeltaTime / LinedBombHit.hitSpeed) {
                material.SetTextureOffset(_MainTex, offset.ChangeX(1f - t / waveSize));
                yield return null;
            }
            
            if (animator)
                yield return animator.WaitPlaying();
            
            renderer.Clear();
            material.SetTextureOffset(_MainTex, offset.ChangeX(-100));
        }

        public struct Callback : IEffectCallback {
            public List<LineInfo> lines;
        }
        
        public class LineInfo {
            public LineInfo(Side side, int offset) {
                this.side = side;
                this.offset = side.Rotate(2).ToInt2() * offset;
            }
            
            public int2 offset;
            public int size;
            public Side side;
            
            public bool Equal(Side side, int offset) {
                return this.side == side && 
                       this.offset == side.Rotate(2).ToInt2() * offset;
                
            }
        }
    }
}