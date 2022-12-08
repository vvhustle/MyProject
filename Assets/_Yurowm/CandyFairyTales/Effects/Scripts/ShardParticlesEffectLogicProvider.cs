using System;
using System.Collections;
using UnityEngine;
using Yurowm.ContentManager;
using Yurowm.Extensions;
using Space = Yurowm.Spaces.Space;

namespace Yurowm.Effects {
    
    public class ShardParticelsEffectLogicProvider : IEffectLogicProvider {

        public bool IsSuitable(EffectBody effectBody) {
            return effectBody.GetComponent<IShardParticles>() != null;
        }

        public IEnumerator Logic(LiveContext context, IEffectCallback[] callbacks, Effect effect) {
            if (!effect.body.SetupComponent(out IShardParticles particles))
                yield break;
            
            var space = context.GetArgument<Space>();

            callbacks?.CastIfPossible<Callback>().ForEach(c => particles.Callback(c));

            particles.Play(space.coroutine);

            while (particles.IsPlaying())
                yield return null;

            particles.Clear();
        }
        
        public struct Callback : IEffectCallback {
            public Vector2 impulse;
            #if PHYSICS_2D
            public Action<Collision2D, PhysicParticles.Particle> onParticleCollision;
            #endif
        }
    }
}