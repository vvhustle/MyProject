using System.Collections.Generic;
using System.Linq;
#if FMOD
using FMOD;
using System;
using FMOD.Studio;
using FMODUnity;
#endif
using UnityEngine;
using Yurowm.Extensions;
using Yurowm.Serialization;
using Yurowm.Sounds;
using AudioSettings = Yurowm.Audio.AudioSettings;
using Debug = UnityEngine.Debug;

namespace Yurowm {
    public class ContentSound : BaseBehaviour, IClipComponent {
        public List<Sound> clips = new List<Sound>();
        static Dictionary<string, float> limiter = new Dictionary<string, float>();
        const float limiterDelay = .03f;
        
        Dictionary<string, string> _clips = new Dictionary<string, string>();
        
        bool isInitialized;
        
        void Initialize() {
            _clips = clips.ToDictionary(x => x.name, x => x.clip);
            
            isInitialized = true;
        }
        
        public void Play(string soundName) {
            if (soundName.IsNullOrEmpty()) return;
                
            if (!isInitialized) Initialize();
            
            if (GameSettings.Instance.GetModule<AudioSettings>().Mute) return;
            
            var clip = _clips.Get(soundName);
            
            if (clip.IsNullOrEmpty()) return;
            
            if (!GetAccessFor(clip)) return;
            
            if (clip.StartsWith("event:/")) {
                #if FMOD
                NewEvent(clip).start();
                #endif
                return;
            }
            
            SoundBase.storage.Items<SoundBase>().FirstOrDefault(s => s.ID == clip)?.Play();
        }
           
        AudioSettings settings;
        
        public void PlayWithParameters(string soundName, params Parameter[] parameters) {
            #if FMOD
            if (soundName.IsNullOrEmpty()) return;
            
            if (!isInitialized) Initialize();
            
            var clip = _clips.Get(soundName);
            if (clip.IsNullOrEmpty()) return;
            if (!GetAccessFor(clip)) return;

            if (settings == null)
                settings = GameSettings.Instance.GetModule<AudioSettings>();
            
            if (!settings.Mute) {
                var soundEffect = NewEvent(clip);
                soundEffect.start();
                soundEffect.release();
            }
            #endif
        }
        
        #if FMOD
        public EventInstance CreateEvent(string eventName) {
            if (eventName.IsNullOrEmpty()) return default;
            
            if (!isInitialized) Initialize();
            
            var clip = _clips.Get(eventName);
            if (clip.IsNullOrEmpty()) return default;
            if (!GetAccessFor(clip)) return default;

            if (settings == null)
                settings = GameSettings.Instance.GetModule<AudioSettings>();
            
            if (settings.Mute) return default;

            return NewEvent(clip);
        }
        
        EventInstance NewEvent(string name) {
            try {
                var e = RuntimeManager.CreateInstance(name);
                Update3DAttributes(e);
                return e;
            } catch (EventNotFoundException) {
                Debug.LogError($"FMOD Event is not found: {name}");
                return default;
            }
        }

        public void Update3DAttributes(EventInstance eventInstance) {
            eventInstance.set3DAttributes(gameObject.transform.position.To3DAttributes());
        }
        #endif

        static bool GetAccessFor(string soundName) {
            if (!limiter.ContainsKey(soundName))
                limiter[soundName] = 0;
            
            if (limiter[soundName] + limiterDelay <= Time.time) {
                limiter[soundName] = Time.time;
                return true;
            }
            
            return false;
        }
        
        public struct Parameter {
            public readonly string name;
            public readonly float value;

            public Parameter(string name, float value) {
                this.name = name;
                this.value = value;
            }
        }
        
        [System.Serializable]
        public class Sound {
            public string name;
            public string clip;

            public Sound(string _name) {
                name = _name;
            }

            public static bool operator ==(Sound a, Sound b) {
                return Equals(a, b);
            }

            public static bool operator !=(Sound a, Sound b) {
                return !Equals(a, b);
            }

            public override bool Equals(object obj) {
                if (obj is Sound sound)
                    return Equals(this, sound);
                return false;
            }
            
            static bool Equals(Sound a, Sound b) {
                return a?.name == b?.name;
            }
        }
    }
}