using System.Collections;
using System.Linq;
using UnityEngine;
using Yurowm.ContentManager;
using Yurowm.Extensions;

namespace Yurowm.Effects {
    public class ParticlesEffectLogicProvider : IEffectLogicProvider {
        
        public bool IsSuitable(EffectBody effectBody) {
            return effectBody.transform
                .AndAllChild()
                .Any(t => t.GetComponent<ParticleSystem>());
        }

        public IEnumerator Logic(LiveContext context, IEffectCallback[] callbacks, Effect effect) {
            
            var particleSystems = effect.body.transform
                .AndAllChild()
                .Select(t => t.GetComponent<ParticleSystem>())
                .Where(p => p != null)
                .ToArray();
            
            if (particleSystems.IsEmpty())
                yield break;
            
            foreach (var ps in particleSystems) {
                var emission = ps.emission; 
                    
                emission.enabled = true;
                if (!ps.isPlaying)
                    ps.randomSeed = (uint) Random.Range(int.MinValue, int.MaxValue);
                    
                ps.Play();
            }
            
            while (particleSystems.Any(ps => ps.IsAlive()))
                yield return null;
            
            particleSystems.ForEach(ps => ps.Clear());
        }
    }
}