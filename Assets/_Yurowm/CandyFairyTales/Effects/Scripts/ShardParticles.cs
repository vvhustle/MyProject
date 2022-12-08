using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Yurowm.Coroutines;
using Yurowm.Extensions;
using Yurowm.Spaces;
using Yurowm.Utilities;

namespace Yurowm.Effects {
    public class ShardParticles : BaseBehaviour, IShardParticles, SpaceTime.ISensitiveComponent {
        
        #if UNITY_EDITOR
        [UnityEngine.ContextMenu("Play")]
        void EditorPlay() {
            DestroyInstances();
            Initialize();
            Play(EditorCoroutine.GetCore());
        }
        #endif

        public FloatRange lifetime = 1;
        
        public FloatRange initialScale = 1;
        public AnimationCurve scaleOverLivetime = AnimationCurve.Linear(0, 0, 1, 1);
        
        public FloatRange initialRotation = 0;
        public FloatRange rotation = new FloatRange(-360, 360);
        public bool randomRotationAsix = false;
        
        public FloatRange initialSpeedX = new FloatRange(-1, 1);
        public FloatRange initialSpeedY = new FloatRange(-1, 1);
        
        public Vector2 gravity = new Vector2(0, -1);
        
        public ParticleInfo[] particleInfos;
        Transform[] particles;

        void Initialize() {
            if (particles != null)
                DestroyInstances();

            particles = particleInfos.SelectMany(i => i.Emit()).ToArray();
        }
        
        Vector2 impulse;
        
        public void Callback(ShardParticelsEffectLogicProvider.Callback callback) {
            impulse = callback.impulse;
        }
        
        public void Play(CoroutineCore core = null) {
            if (particles == null)
                Initialize();
                
            if (particles.Length == 0)
                return;
            
            Playing().Run(core);
        }

        List<Particle> liveParticles = new List<Particle>();
        
        public bool IsPlaying() {
            return liveParticles.Count > 0;
        }
        
        public void Clear() {
            if (particles == null)
                Initialize();
            
            liveParticles.Clear();
            particles.ForEach(p => p.gameObject.SetActive(false));
        }

        void DestroyInstances() {
            var origianal = particleInfos.Select(i => i.transform).ToArray();
            particles
                .Where(p => !origianal.Contains(p))
                .ForEach(Destroy);
            particles = null;
        }
        
        SpaceTime time = null;

        public void OnChangeTime(SpaceTime time) {
            this.time = time;
        }
        
        IEnumerator Playing() {
            liveParticles.Clear();
            liveParticles.AddRange(particles.Select(NewParticle));
            
            while (liveParticles.Count > 0) {
                yield return null;
                
                liveParticles.RemoveAll(p => {
                    UpdateParticle(p, time?.Delta ?? Time.deltaTime);
                    return !p.isAlive;
                });
            }
            
            if (!Application.isPlaying)
                DestroyInstances();
        }
        
        Particle NewParticle(Transform transform) {
            var result = new Particle();
            
            result.transform = transform;
                    
            result.time = 0;
            result.lifetime = YRandom.main.Range(lifetime);
                    
            result.position = Vector2.zero;
            result.rotation = YRandom.main.Range(initialRotation);
            if (randomRotationAsix)
                result.asix = Quaternion.Euler(
                    YRandom.main.Range(0f, 360f),
                    YRandom.main.Range(0f, 360f),
                    YRandom.main.Range(0f, 360f)) 
                    * result.asix;

            result.speed = new Vector2(YRandom.main.Range(initialSpeedX), YRandom.main.Range(initialSpeedY));
            result.speed += impulse;
            result.rotationSpeed = YRandom.main.Range(rotation);
            
            result.scale = YRandom.main.Range(initialScale);
            
            UpdateParticle(result, 0);
            
            return result;
        }
        
        void UpdateParticle(Particle particle, float deltaTime) {
            particle.time += deltaTime;
            
            particle.transform.gameObject.SetActive(particle.isAlive);

            if (!particle.isAlive) return;
            
            particle.speed += gravity * deltaTime;
            particle.position += particle.speed * deltaTime;
            particle.rotation += particle.rotationSpeed * deltaTime;

            particle.transform.localPosition = particle.position;
            particle.transform.localRotation = Quaternion.AngleAxis(particle.rotation, particle.asix);
            particle.transform.localScale = Vector3.one * 
                                            (particle.scale * scaleOverLivetime
                                                 .Evaluate(particle.time / particle.lifetime));
        }

        [Serializable]
        public struct ParticleInfo {
            public Transform transform;
            public int count;

            public IEnumerable<Transform> Emit() {
                if (count <= 0) yield break;
                yield return transform;
                for (int i = 1; i < count; i++) {
                    var instance = Instantiate(transform, transform.parent, true);
                    instance.name = transform.name;
                    yield return instance;
                }
            }
        }

        class Particle {
            public Transform transform;
            public float lifetime;
            public float time;
            
            public bool isAlive => time <= lifetime;
            
            public Vector2 position;
            public float rotation;
            
            public Vector2 speed;
            public float rotationSpeed;
            
            public float scale;
            public Vector3 asix = Vector3.forward;

            public void Update(float deltaTime) {
                time += deltaTime;
            }
        }
    }

    public interface IShardParticles {
        void Play(CoroutineCore coroutine);
        bool IsPlaying();
        void Clear();
        
        void Callback(ShardParticelsEffectLogicProvider.Callback callback);
    }
}

