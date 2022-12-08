using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.Scripting;
using Yurowm.Coroutines;
using Yurowm.Extensions;

[assembly: Preserve]
namespace Yurowm.Utilities {
    public class OnLaunchAttribute : Attribute {
        readonly int _order;
        
        public OnLaunchAttribute(int order = 0) {
            _order = order;
        }

        static OnLaunchModifier[] launchModifiers = null;
        public static Action unload = delegate {};
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        public static void PreLaunch() {
            launchModifiers = Utils.FindInheritorTypes<OnLaunchModifier>(true)
                            .Select(t => {
                                try {
                                    return (OnLaunchModifier) Activator.CreateInstance(t);
                                } catch (Exception e) {
                                    UnityEngine.Debug.LogException(e);
                                }
                                return null;
                            })
                            .NotNull().ToArray();
            
            launchModifiers.ForEach(m => {
                try {
                    m.BeforeSceneLoaded();
                } catch (Exception e) {
                    UnityEngine.Debug.LogException(e);
                }
            });
        }
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void Launch() {
            Utils.SetMainThread();
            Launching().Run();
        }

        static IEnumerator Launching() {
            foreach (var modifier in launchModifiers) 
                yield return modifier.PreLoad();
            
            var report = new StringBuilder();
            
            using (var executionTimer = new ExecutionTimer("OnLaunch", r => report.Append(r))) {
                var launches = UnityUtils.GetAllMethodsWithAttribute<OnLaunchAttribute>(
                        BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                    .ToDictionary();
                
                executionTimer.Flash("Reflection");
                
                var parameters = new object[0];
                
                unload?.Invoke();
                unload = delegate {};

                foreach (var l in launches.OrderBy(l => l.Value._order)) {
                    object result = null;
                    try {
                        result = l.Key.Invoke(null, parameters);
                    } catch (Exception e) {
                        UnityEngine.Debug.LogException(e);
                    }
    
                    if (result is IEnumerator enumerator)
                        yield return enumerator;
                    
                    executionTimer.Flash($"{l.Key.DeclaringType?.FullName}:{l.Key.Name}");
                }
            }
            
            if (report != null) 
                UnityEngine.Debug.Log(report.ToString());

           
            foreach (var modifier in launchModifiers)
                yield return modifier.PostLoad();            
        }
    }
    
    [JITMethodIssueType]
    public abstract class OnLaunchModifier {
        public abstract void BeforeSceneLoaded();
        public abstract IEnumerator PreLoad();
        public abstract IEnumerator PostLoad();
    }
    
    public static class OnceAccess {
        static List<string> keys = new List<string>();
        
        public static bool GetAccess(string key) {
            if (keys.Contains(key))
                return false;
            keys.Add(key);
            return true;
        }
    }
}
