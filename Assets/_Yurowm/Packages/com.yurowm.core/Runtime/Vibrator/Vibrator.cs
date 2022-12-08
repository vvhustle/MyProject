using System;
using System.Collections;
using UnityEngine;
using Yurowm.Coroutines;
using Yurowm.DebugTools;
using Yurowm.Utilities;
using System.Linq;
using System.Runtime.InteropServices;

namespace Yurowm {
    public static class Vibrator {
        
        public static bool Active = true;
        
        [OnLaunch(1000)]
        public static void Start() {
            if (!OnceAccess.GetAccess("Vibrator")) 
                return;
            
            if (Application.isEditor)
                return;

            DebugPanel.Log("Vibrate", "Vibrator", Vibrate);
            
            if (Application.platform == RuntimePlatform.Android) {
                float[] time = {0.01f, 0.03f, 0.1f, 0.2f, 0.5f, 0.7f, 1f, 2f};
                foreach (var _time in time) {
                    var t = _time;   
                    DebugPanel.Log($"Vibrate {_time} s.", "Vibrator", () => AndroidVibrate(t));
                }
                
                #if UNITY_ANDROID
                AndroidVibratorLogic().Run();
                #endif
            }
            
            
            if (Application.platform == RuntimePlatform.IPhonePlayer)
                foreach (var type in Enum.GetValues(typeof(iOSVibrateType)).Cast<iOSVibrateType>()) {
                    var t = type;   
                    DebugPanel.Log($"Vibrate {type}", "Vibrator", () => iOSVibrate(t));
                }
        }
        
        #if !UNITY_EDITOR
        
        #if UNITY_ANDROID
        static AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        static AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        static AndroidJavaObject vibrator =currentActivity.Call<AndroidJavaObject>("getSystemService", "vibrator");
        static AndroidJavaObject context = currentActivity.Call<AndroidJavaObject>("getApplicationContext");
        #endif
        
        #if UNITY_IOS
        [DllImport ( "__Internal" )]
        static extern bool _HasVibrator ();

        [DllImport ( "__Internal" )]
        static extern void _Vibrate ();

        [DllImport ( "__Internal" )]
        static extern void _VibratePop ();

        [DllImport ( "__Internal" )]
        static extern void _VibratePeek ();

        [DllImport ( "__Internal" )]
        static extern void _VibrateNope ();
        #endif

        #endif
        
        public static void Vibrate() {
            if (!Active) return;
            AndroidVibrate(.1f);
            iOSVibrate();
        }
        
        public static void VibrateWithPower(float power) {
            AndroidVibrate(.02f);
            
            iOSVibrate(power <.7f ? iOSVibrateType.Pop : iOSVibrateType.Peek);
        }
        
        public static void UnityVibrate() {
            #if UNITY_IOS || UNITY_ANDROID
            if (!Active) return;
            Handheld.Vibrate();
            #endif
        }
        
        #if UNITY_ANDROID
        
        static DateTime androidVibrateUntil;

        static IEnumerator AndroidVibratorLogic() {
            #if UNITY_EDITOR
            yield break;
            #else
            while (true) {
                if (Active && androidVibrateUntil > DateTime.Now) {
                    var until = androidVibrateUntil;
                    vibrator.Call("vibrate", (long) (androidVibrateUntil - DateTime.Now).Milliseconds);
                    while (until > DateTime.Now)
                        yield return null; 
                }

                yield return null;
            }
            #endif
        }
        
        #endif
        
        public static void AndroidVibrate(float seconds = 0.5f) {
            #if UNITY_ANDROID
            if (!Active) return;
            
            var stopTimer = DateTime.Now.AddSeconds(seconds);
            if (androidVibrateUntil < stopTimer)
                androidVibrateUntil = stopTimer;
            #endif
        }
        
        public enum iOSVibrateType {
            Pop = 0,
            Peek = 1,
            Nope = 2
        }
        
        public static void iOSVibrate(iOSVibrateType type = iOSVibrateType.Pop) {
            if (!Active) return;
            #if UNITY_IOS && !UNITY_EDITOR
            switch (type) {
                case iOSVibrateType.Pop: _VibratePop(); return;
                case iOSVibrateType.Peek: _VibratePeek(); return;
                case iOSVibrateType.Nope: _VibrateNope(); return;
            }
            #endif
        }
        
        public static void AndroidVibrateCancel() {
            #if UNITY_ANDROID && !UNITY_EDITOR
            vibrator.Call("cancel");
            #endif
        }
        
        public static bool HasVibrator() {
            #if !UNITY_EDITOR
        
            #if UNITY_ANDROID
		    AndroidJavaClass contextClass = new AndroidJavaClass("android.content.Context");
		    string Context_VIBRATOR_SERVICE = contextClass.GetStatic<string>("VIBRATOR_SERVICE");
		    AndroidJavaObject systemService = context.Call<AndroidJavaObject>("getSystemService", Context_VIBRATOR_SERVICE);
			return systemService.Call<bool>("hasVibrator");
            #endif

            #if UNITY_IOS
            return _HasVibrator();
            #endif
            
            #endif    
            
            return false;
        }
    }
}