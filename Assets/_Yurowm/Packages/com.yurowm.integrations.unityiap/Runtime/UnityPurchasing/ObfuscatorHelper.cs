using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Yurowm.Extensions;
using Yurowm.Utilities;

namespace Yurowm.Services {
    public class ObfuscatorHelper {
        public enum Tangle {
            GooglePlay,
            Apple
        }
        
        static Dictionary<Tangle, MethodInfo> dataMethods = new Dictionary<Tangle, MethodInfo>();
        
        static readonly object[] emptyParameters = new object[0];
        
        static Type GetTangleType(Tangle tangle) {
            var path = $"UnityEngine.Purchasing.Security.{tangle}Tangle";
            return AppDomain.CurrentDomain
                .GetAssemblies()
                .Select(a => a.GetType(path))
                .NotNull()
                .FirstOrDefault();
        }
        
        static byte[] GetData(Tangle tangle) {
            if (!dataMethods.TryGetValue(tangle, out var method)) {
                method = GetTangleType(tangle)?.GetMethod("Data", BindingFlags.Static | BindingFlags.Public);
                dataMethods.Add(tangle, method);
            }
            
            return (byte[]) method?.Invoke(null, emptyParameters); 
        }
        
        #if UNITY_IAP_OBFUSCATION
        
        public static UnityEngine.Purchasing.Security.CrossPlatformValidator GetValidator() {
            try {
                return new UnityEngine.Purchasing.Security.CrossPlatformValidator(
                    GetData(Tangle.GooglePlay), 
                    GetData(Tangle.Apple), 
                    Application.identifier);
            } catch (NotImplementedException) { }
            return null;
        }
        
        [JITMethodIssueTypeProvider]
        static IEnumerator<Type> GetJITMITypes() {
            foreach (var tangle in Enum.GetValues(typeof(Tangle)).Cast<Tangle>()) {
                var type = GetTangleType(tangle);
                if (type != null) yield return type;
            }
        }
        
        #else
        
        public static int GetValidator() {
            return 0;
        }
        
        #endif
    }

    public class UnityIAPObfuscationSymbol : ScriptingDefineSymbolAuto {
        public override IEnumerable<Platform> GetSupportedPlatforms() {
            #if UNITY_IAP
            yield return Platform.Android;
            yield return Platform.iOS;
            yield return Platform.tvOS;
            yield return Platform.OSX;
            #else
            yield break;
            #endif
        }

        public override IEnumerable<string> GetRequiredPackageIDs() {
            yield return "com.unity.purchasing";
        }

        public override string GetSybmol() {
            return "UNITY_IAP_OBFUSCATION";
        }
    }
}