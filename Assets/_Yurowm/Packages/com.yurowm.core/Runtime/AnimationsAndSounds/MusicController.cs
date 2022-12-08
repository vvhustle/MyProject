using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if FMOD
using FMOD;
using FMOD.Studio;
using FMODUnity;
#endif
using Yurowm.Coroutines;
using Yurowm.Extensions;
using Yurowm.Serialization;
using Yurowm.Utilities;

namespace Yurowm.Audio {
    public class MusicController : SettingsModule {
        
        [OnLaunch(1000)]
        public static void InitializeOnLoad() {
            // Just for starting music
            GameSettings.Instance.GetModule<MusicController>();
        }
        
        CoroutineThread thread = new CoroutineThread("Music");
        
        public bool musicEnabled = true;
        
        float targetVolume => musicEnabled ? 1f : 0f;
        
        public const string musicVolumeParameter = "Volume";
        const string musicEventPath = "event:/Music/";
        
        Dictionary<string, List<string>> musicEventNames = new Dictionary<string, List<string>>();

        string currentPlace = null;
        
        #if FMOD
        public EventInstance currentMusicEvent;
        #endif
        
        public override void Initialize() {
            #if FMOD
            thread.Run();
            #endif
        }
        
        void OnPlaceRequest(string place) {
            if (place.IsNullOrEmpty()) return;
            
            if (musicEventNames.ContainsKey(place)) return;
            
            var list = new List<string>();
            
            musicEventNames.Add(place, list);
            
            #if FMOD
            try {
                for (int i = 1;; i++) {
                    string eventName = musicEventPath + place + i;
                        
                    if (RuntimeManager.GetEventDescription(eventName).isValid())
                        list.Add(eventName);
                }
            } catch (EventNotFoundException) {}
            #endif
        }
        
        public void SetMusicEnable(bool value) {
            #if FMOD
            if (musicEnabled != value) {
                musicEnabled = value;
                SetDirty();
            }
            
            thread.AddInQueue(MusicFade(currentMusicEvent));
            #endif
        }
        
        public void Play(string place, bool force = false, params Parameter[] parameters) {
            if (!force && place == currentPlace) {
                #if FMOD
                ChangeParameters(currentMusicEvent, parameters);
                #endif
                
                return;
            }
            
            OnPlaceRequest(place);
            
            currentPlace = place;
            
            #if FMOD
            thread.AddInQueue(MusicPlay(currentPlace, parameters));
            #endif
        }

        IEnumerator MusicPlay(string place, params Parameter[] parameters) {
            #if FMOD
            EventInstance nextMusicEvent;

            if (musicEventNames[place].Count == 0) {
                nextMusicEvent = default;
            } else {
                nextMusicEvent = RuntimeManager.CreateInstance(musicEventNames[place].GetRandom());
                
                if (nextMusicEvent.getParameter(musicVolumeParameter, out var volume) == RESULT.OK)
                    volume.setValue(0);
                
                nextMusicEvent.start();
            }
            
            
            yield return MusicFade(nextMusicEvent, targetVolume, parameters)
                .With(MusicFade(currentMusicEvent, 0))
                .Parallel();
            
            if (currentMusicEvent.isValid()) {
                currentMusicEvent.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                currentMusicEvent.release();
            }

            currentMusicEvent = nextMusicEvent;
            
            ChangeParameters(currentMusicEvent, parameters);
            #else
            yield break;
            #endif
        }

        #if FMOD
        
        void ChangeParameters(EventInstance e, Parameter[] parameters) {
            if (parameters.IsEmpty()) return;
            foreach (var parameter in parameters)
                if (e.getParameter(parameter.name, out var p) == RESULT.OK)
                    p.setValue(parameter.value);
        }
        
        IEnumerator MusicFade(EventInstance musicEvent, float targetVolume = -1, params Parameter[] parameters) {
            if (!musicEvent.isValid())
                yield break;
            
            if (targetVolume == -1)
                targetVolume = this.targetVolume;
            
            ChangeParameters(musicEvent, parameters);
            
            if (musicEvent.getParameter(musicVolumeParameter, out var parameter) == RESULT.OK) {
                parameter.getValue(out var currentVolume);
                
                while (currentVolume != targetVolume) {
                    currentVolume = Mathf.MoveTowards(currentVolume, targetVolume, Time.unscaledDeltaTime * 2f);                  
                    if (parameter.setValue(currentVolume) != RESULT.OK)
                        break;

                    yield return null;
                }
            }
        }
        #endif
        
        public struct Parameter {
            public readonly string name;
            public readonly float value;

            Parameter(string name, float value) {
                this.name = name;
                this.value = value;
            }
            
            public static Parameter New(string name, float value) {
                return new Parameter(name, value);    
            }
        }
        
        public override void Serialize(Writer writer) {
            writer.Write("music", musicEnabled);
        }

        public override void Deserialize(Reader reader) {
            reader.Read("music", ref musicEnabled);
        }
    }
}