using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using Yurowm.Core;
using Yurowm.Coroutines;
using Yurowm.Extensions;
using Yurowm.Serialization;

namespace Yurowm.Sounds {
    public static class SoundController {
        
        const string coreName = "YSounds";
        
        const HideFlags hideFlags = HideFlags.DontSave | HideFlags.HideInHierarchy;
        
        static void BuildCore() {
            core = new GameObject(coreName);
            core.transform.Reset();
            core.hideFlags = hideFlags;
            
            sfxSource = core.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
            sfxSource.rolloffMode = AudioRolloffMode.Linear;

            if (Application.isPlaying) {
                var listener = new GameObject(nameof(AudioListener));
                listener.AddComponent<AudioListener>();
                listener.hideFlags = HideFlags.HideInHierarchy | HideFlags.NotEditable;
            }
            
            specialSources.Clear();
            clips.Clear();    
        }

        static GameObject core;
        static AudioSource sfxSource;
        static AudioSource musicSource;
        static List<AudioSource> specialSources = new List<AudioSource>();

        static float _SoundVolume = 1;
        public static float SoundVolume {
            get => _SoundVolume;
            set {
                _SoundVolume = value.Clamp01();
                if (sfxSource)
                    sfxSource.volume = _SoundVolume;
            }
        }
        
        public static float MusicVolumeMultiplier = 1f;
        
        static float _MusicVolume = 1;
        public static float MusicVolume {
            get => _MusicVolume;
            set {
                _MusicVolume = value.Clamp01();
                if (musicSource)
                    musicSource.volume = _MusicVolume * MusicVolumeMultiplier;
            }
        }
        
        public static void PlayEffect(AudioClip clip) {
            
            #if UNITY_EDITOR
            if (!Application.isPlaying) {
                PlayEffectSpecialSource(clip, out _);
                return;
            }
            #endif
            
            if (clip && sfxSource) {
                sfxSource.PlayOneShot(clip);
            }
        }
        
        public static void PlayEffectSpecialSource(AudioClip clip, out AudioSource source) {
            source = null;
            
            if (!clip) return;
            
            if (!core)
                BuildCore();

            #if UNITY_EDITOR
            if (!Application.isPlaying) {
                if (specialSources.Any(s => !s))
                    BuildCore();
                source = sfxSource;
                source.Stop();
            } else
            #endif
                source = specialSources.FirstOrDefault(s => !s.isPlaying);
            
            if (!source) {
                var go = new GameObject("SpecialSource");
                go.transform.SetParent(core.transform);
                go.transform.Reset();
                go.hideFlags = hideFlags;
                source = go.AddComponent<AudioSource>();
                source.rolloffMode = AudioRolloffMode.Linear;
                specialSources.Add(source);
            }
            
            source.clip = clip;
            source.volume = SoundVolume;
            source.pitch = 1f;
            source.loop = false;
            source.Play();
        }

        public static void PlayMusic(AudioClip clip) {
            if (clip == null)
                return;
            
            if (musicSource && musicSource.isPlaying && musicSource.clip == clip)
                return;

            PlayEffectSpecialSource(clip, out var source);
            
            StopMusic();
            
            musicSource = source;
            musicSource.loop = true;
            musicSource.volume = 0;
            
            Fade(musicSource, MusicVolume * MusicVolumeMultiplier).Run();
        }

        static IEnumerator Fade(AudioSource source, float volume) {
            if (!source) yield break;
            var startVolume = source.volume;
            for (var t = 0f; t < 1f; t += Time.unscaledDeltaTime / 2) {
                source.volume = YMath.Lerp(startVolume, volume, t);
                yield return null;
            }
            source.volume = volume;
        }

        public static void StopMusic() {
            if (!musicSource) return;
            
            var source = musicSource;

            Fade(source, 0f)
                .ContinueWith(source.Stop)
                .Run();
        }

        public const string RootFolderName = "Sounds";
        static string _rootFolderPath;
        static string rootFolderPath {
            get {
                if (_rootFolderPath.IsNullOrEmpty())
                    _rootFolderPath = Path.Combine(TextData.GetPath(TextCatalog.StreamingAssets), RootFolderName); 
                return _rootFolderPath;
            }
                
        }


        #region Clips

        static Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();
        static List<string> ignore = new List<string>();
        
        public static IEnumerator GetClip(string clipPath, Action<AudioClip> getResult) {
            if (getResult == null || clipPath.IsNullOrEmpty() || ignore.Contains(clipPath))
                yield break;
            
            if (clips.TryGetValue(clipPath, out var clip)) {
                if (clip != null) {
                    getResult.Invoke(clip);
                    yield break;
                }

                clips.Clear();
            }
            
            string path = null;
            
            if (!Application.isEditor) {
                path = Application.streamingAssetsPath;
                #if UNITY_IOS
                path = "file://" + path;
                #endif
                path = Path.Combine(path, RootFolderName, clipPath);
            } else {
                path = Path.Combine(rootFolderPath, clipPath);
                if (!File.Exists(path))
                    yield break;
                switch (SystemInfo.operatingSystemFamily) {
                    case OperatingSystemFamily.Windows:
                        path = "file:///" + path; break;
                    default:
                        path = "file://" + path; break;
                }
            }
            
            using (var request = UnityWebRequestMultimedia.GetAudioClip(path, AudioType.UNKNOWN)) {
                var operation = request.SendWebRequest();
                
                yield return operation;

                switch (request.result) {
                    case UnityWebRequest.Result.Success:
                        clip = DownloadHandlerAudioClip.GetContent(request);
                        if (clip) {
                            clips[clipPath] = clip;
                            getResult.Invoke(clip);
                        }    else
                            ignore.Add(clipPath);
                        yield break;
                    case UnityWebRequest.Result.ConnectionError:
                    case UnityWebRequest.Result.ProtocolError:
                    case UnityWebRequest.Result.DataProcessingError: {
                        UnityEngine.Debug.LogError($"{request.result}: {request.error}");
                        break;
                    }
                }
            }
        }
        
        #endregion
    }
}