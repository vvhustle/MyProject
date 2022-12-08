#if PHYSICS_2D
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Yurowm.Extensions;
#endif
using UnityEngine;
using Yurowm.ContentManager;
using Yurowm.Coroutines;
using Yurowm.Spaces;
using Yurowm.Utilities;
using ParticleInfo = Yurowm.Effects.ShardParticles.ParticleInfo;

namespace Yurowm.Effects {
    public class PhysicParticles : BaseBehaviour, IReserved, IShardParticles, SpaceTime.ISensitiveComponent {
        
        public ParticleInfo[] particleInfos;
        
        public FloatRange lifetime = 1;
        
        public FloatRange initialPositionOffset;
        
        public FloatRange initialSpeedX = new FloatRange(-1, 1);
        public FloatRange initialSpeedY = new FloatRange(-1, 1);
        public FloatRange rotation = new FloatRange(-360, 360);
        
        public FloatRange initialScale = 1;
        public AnimationCurve scaleOverLivetime = AnimationCurve.Linear(0, 0, 1, 1);

        #if PHYSICS_2D
        
        Rigidbody2D[] particles;

        void Initialize() {
            if (particles != null)
                DestroyInstances();

            particles = particleInfos
                .SelectMany(i => i.Emit())
                .Select(t => t.GetComponent<Rigidbody2D>())
                .NotNull()
                .ToArray();
        }
        
        Vector2 impulse;
        
        public void Callback(ShardParticelsEffectLogicProvider.Callback callback) {
            impulse = callback.impulse;
            if (callback.onParticleCollision != null)
                onParticleCollision += callback.onParticleCollision;
            
        }
        
        public void Play(CoroutineCore core = null) {
            #if PHYSICS_2D
            if (particles == null)
                Initialize();
                
            if (particles.Length == 0)
                return;

            Playing().Run(core);
            #endif
        }
        

        
        public void Clear() {
            if (particles == null)
                Initialize();
            
            liveParticles.Clear();
            particles.ForEach(p => p.gameObject.SetActive(false));
        }

        public bool IsPlaying() {
            return liveParticles.Count > 0;
        }
        
        SpaceTime time = null;

        public void OnChangeTime(SpaceTime time) {
            this.time = time;
        }
        
        List<Particle> liveParticles = new List<Particle>();

        IEnumerator Playing() {
            IgnoreCollisions(true);

            float ignoringCollsionsTime = .3f;

            liveParticles.Clear();
            liveParticles.AddRange(particles.Select(NewParticle));
            
            while (liveParticles.Count > 0) {
                yield return null;
                
                if (ignoringCollsionsTime > 0) {
                    ignoringCollsionsTime -= time?.Delta ?? Time.deltaTime;
                    if (ignoringCollsionsTime <= 0) 
                        IgnoreCollisions(false);
                }
                
                liveParticles.RemoveAll(p => {
                    UpdateParticle(p, time?.Delta ?? Time.deltaTime);
                    return !p.isAlive;
                });
            }
            
            if (!Application.isPlaying)
                DestroyInstances();
        }
        
        public Action<Collision2D, Particle> onParticleCollision = delegate {};
        
        Particle NewParticle(Rigidbody2D rigidbody) {

            var result = new Particle();
            
            if (rigidbody.SetupComponent(out Collision2DEvent collisionEvent))
                collisionEvent.onEnter = collision => onParticleCollision.Invoke(collision, result);

            result.rigidbody = rigidbody;
            rigidbody.gameObject.SetActive(true);
            
            rigidbody.velocity = new Vector2(
                YRandom.main.Range(initialSpeedX),
                YRandom.main.Range(initialSpeedY));
            
            rigidbody.transform.position = transform.position.To2D() 
                                           + rigidbody.velocity.FastNormalized() *
                                           YRandom.main.Range(initialPositionOffset);
            
            rigidbody.velocity += impulse;
            
            rigidbody.rotation = YRandom.main.Range(0f, 360f);

            rigidbody.angularVelocity = YRandom.main.Range(rotation);

            result.time = 0;
            result.lifetime = YRandom.main.Range(lifetime);

            result.scale = YRandom.main.Range(initialScale);
            
            UpdateParticle(result, 0);
            
            return result;
        }
        
        void UpdateParticle(Particle particle, float deltaTime) {
            particle.time += deltaTime;
            
            particle.rigidbody.gameObject.SetActive(particle.isAlive);

            if (!particle.isAlive) return;
            
            particle.rigidbody.transform.localScale = Vector3.one * 
                                            (particle.scale * scaleOverLivetime
                                                 .Evaluate(particle.time / particle.lifetime));
        }
        
        void IgnoreCollisions(bool value) {
            particles
                .Select(p => p.GetComponent<Collider2D>())
                .ToArray()
                .ForEachPair((a, b) => Physics2D.IgnoreCollision(a, b, value));
        }
        
        public class Particle {
            public Rigidbody2D rigidbody;
            public float lifetime;
            public float time;
            public float scale;

            public bool isAlive => time <= lifetime && rigidbody.gameObject.activeSelf;
        }
        
        void DestroyInstances() {
            var origianal = particleInfos
                .Select(i => i.transform)
                .ToArray();
            particles
                .Where(p => !origianal.Contains(p.transform))
                .ForEach(Destroy);
            particles = null;
        }
        
        public void Rollout() {
            Clear();
        }
        
        #else
        
        public void Rollout() { }

        public void Play(CoroutineCore coroutine) { }

        public bool IsPlaying() => false;

        public void Clear() { }

        public void Callback(ShardParticelsEffectLogicProvider.Callback callback) { }

        public void OnChangeTime(SpaceTime time) { }
        
        #endif
    }
}