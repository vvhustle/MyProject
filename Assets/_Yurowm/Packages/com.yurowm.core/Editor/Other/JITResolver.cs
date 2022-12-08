using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using Yurowm.Extensions;
using Yurowm.Utilities;

namespace Yurowm {
    public class JITMethodIssueResolver : IPreprocessBuildWithReport, IPostprocessBuildWithReport {
        
        public int callbackOrder => 0;

        const string fileName = "JITMethodIssue.cs";

        const string pattern = @"// Auto generated script. Don't change it.

using UnityEngine;

public class JITMethodIssue : MonoBehaviour {

	#if UNITY_EDITOR
	[UnityEditor.InitializeOnLoadMethod]
	static void RemoveOnLoad() { 
		var path = System.IO.Path.Combine(Application.dataPath, """ + fileName + @""");
        System.IO.File.Delete(path);
        System.IO.File.Delete(path + "".meta"");
        UnityEditor.AssetDatabase.Refresh();
    }
    #endif

	void Awake() {
		Destroy(this);

        if (Mathf.Abs(0) < 1) return;

        var p = new object();

*METHODS*
    }
}";
        
        
        [MenuItem("Yurowm/Tools/JITMIR Code to Clipboard")]
        static void JITMIRCodeToClipboard() {
            EditorGUIUtility.systemCopyBuffer = new JITMethodIssueResolver().GenerateCode();
        }
        
        string GenerateCode() {
            StringBuilder lines = new StringBuilder();

            GetAllLines().Distinct().ForEach(l => lines.AppendLine($"\t\t{l};"));
            
            return pattern.Replace("*METHODS*", lines.ToString());
        }

        
        public void OnPreprocessBuild(BuildReport report) {
            if (PlayerSettings.GetScriptingBackend(EditorUserBuildSettings.selectedBuildTargetGroup) !=
                ScriptingImplementation.IL2CPP) return;

            var inBuildScript = new FileInfo(Path.Combine(Application.dataPath, fileName));
            
            var code = GenerateCode();
            
            if (inBuildScript.Exists)
                inBuildScript.Delete();
            
            File.WriteAllText(inBuildScript.FullName, code);
            
            AssetDatabase.Refresh(ImportAssetOptions.Default);
        }
        
        public void OnPostprocessBuild(BuildReport report) {
            var inBuildScript = new FileInfo(Path.Combine(Application.dataPath, fileName));

            if (!inBuildScript.Exists) return;
            
            inBuildScript.Delete();
            AssetDatabase.Refresh(ImportAssetOptions.Default);
        }
        
        #region Utils

        static Type[] providedTypes;

        static IEnumerable<Type> GetAllTypes() {
            if (providedTypes == null) {
                Type returnType = typeof(IEnumerator<Type>);

                var _types = AppDomain.CurrentDomain
                    .GetAssemblies()
                    .SelectMany(a => a.GetTypes())
                    .SelectMany(t => t.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                    .Where(m => m.ReturnType == returnType 
                                && m.GetCustomAttributes(true).CastOne<JITMethodIssueTypeProvider>() != null)
                    .SelectMany(m => {
                        var a = m.GetCustomAttributes(true).CastOne<JITMethodIssueTypeProvider>();
                        a.SetMethod(m);
                        return a.GetTypes();
                    }).ToList();
                
                _types.AddRange(AppDomain.CurrentDomain
                    .GetAssemblies()
                    .SelectMany(a => a.GetTypes())
                    .Where(t => t.GetAttribute<JITMethodIssueTypeAttribute>() != null));
                
                providedTypes = _types
                    .Distinct()
                    .NotNull()
                    .ToArray();
            }

            foreach (var type in providedTypes)
                yield return type;

            bool FilterJITMIType(Type type) {
                if (!type.IsPublic && !type.IsNestedPublic) return false;
                if (providedTypes.Any(t => t.IsAssignableFrom(type))) return true;
                return false;
            }
            
            var types = AppDomain.CurrentDomain
                .GetAssemblies()
                .Where(AssemblyFilter)
                .SelectMany(a => a.GetTypes())
                .Where(FilterJITMIType)
                .OrderBy(t => t.FullName);
            
            foreach (var type in types)
                yield return type;
        }
        
        
        static bool AssemblyFilter(Assembly assembly) {
            var assemblyName = assembly.FullName;
            if (assemblyName.Contains("Editor") || assemblyName.Contains("Tests"))
                return false;
            if (!assemblyName.StartsWith("Assembly-CSharp") && !assemblyName.Contains("yurowm"))
                return false;
            return true;
        }
        
        static bool FilterType(Type type) {
            if (!type.IsPublic && !type.IsNestedPublic) return false;
            if (type.GetAttribute<JITMethodIssueTypeAttribute>() != null) return true;
            if (type.Namespace?.Contains("Unity") ?? false) return false;
            if (type.GetAttribute<ObsoleteAttribute>() != null) return false;
            if (type.FullName == "JITMethodIssue") return false;
            return true;
        }
        
        // static bool TypeInstanceFilter(Type type) {
        //     if (type.IsAbstract && !type.IsSealed) return false;
        //     return TypeFilter(type);
        // }
        
        static bool MemberFilter(MemberInfo member) {
            var declaringType = member.DeclaringType;
            
            if (!FilterType(declaringType) || !AssemblyFilter(declaringType.Assembly))
                if (providedTypes.All(t => !t.IsAssignableFrom(declaringType)))
                    return false;
            
            if (member is MethodInfo) {
                var name = member.Name;
                if (name.StartsWith("get_")) return false;
                if (name.StartsWith("set_")) return false;
                if (name.StartsWith("op_")) return false;
                if (name.StartsWith("add_")) return false;
                if (name.StartsWith("remove_")) return false;
            }

            return true;
        }
        
        public static IEnumerable<string> GetAllLines() {
            varIndex = 0;
            foreach (var type in GetAllTypes()) {
                foreach (var line in EmitConstructors(type))
                    yield return line;

                foreach (var line in EmitMethods(type)) 
                    yield return line;
            }
        }

        static readonly Type delegateType = typeof(Delegate);
        
        static IEnumerable<string> EmitConstructors(Type type) {
            if (type.IsAbstract || delegateType.IsAssignableFrom(type))
                yield break;
            
            
            var constructors = type
                .GetConstructors()
                .Where(c => c.IsPublic && c.GetCustomAttribute<JITMethodIssueTypeAttribute>() == null);
            
            foreach (var constructor in constructors) {
                string line = ConstructorToLine(constructor);
                if (line.IsNullOrEmpty() || line.Contains("&")) continue;
                line = reGA.Replace(line, ""); 
                yield return line;
            } 
        }

        static string ConstructorToLine(ConstructorInfo constructor) {
            if (constructor.IsGenericMethodDefinition)
                return "";
            
            try {
                string result = "p = new " + TypeToLine(constructor.DeclaringType);
                
                result += "(" + string.Join(", ", constructor.GetParameters()
                              .Select(x => $"({TypeToLine(x.ParameterType)}) p").ToArray()) + ")";
                
                return result;
            }
            catch (Exception) {
                return "";
            }
        }

        static IEnumerable<string> EmitMethods(Type type) {
            var methods = type
                    .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance)
                    .Where(MemberFilter)
                    .OrderBy(m => m.IsStatic)
                    .ThenBy(m => m.Name);
            
            foreach (var method in methods) {
                var line = MethodToLine(method);
                if (!line.IsNullOrEmpty()) 
                    yield return line;
            } 
        }

        static string MethodToLine(MethodInfo method) {
            try {
                string result = "";
                
                if (method.IsStatic)
                    result += TypeToLine(method.DeclaringType);
                else
                    result += $"(({TypeToLine(method.DeclaringType)}) p)";
                
                result += "." + method.Name;
                
                Type[] genericArguments = method.GetGenericArguments();
                if (genericArguments.Length > 0)
                    result += $"<{genericArguments.Select(TypeToLine).Join(", ")}>";

                result += "(" + method.GetParameters().Select(ParameterToLine).Join(", ") + ")";
                
                
                if (result.IsNullOrEmpty() || result.Contains("&")) 
                    return null;
                result = reGA.Replace(result, ""); 
                
                return result;
            }
            catch (Exception e) {
                return "";
            }
        }
        
        static readonly Type rootType = typeof(object);
        
        static Type NoGenericParameterType(Type type) {
            if (type.IsGenericParameter) {
                var result = type.BaseType;

                if (result != null && result != rootType && result.IsAssignableFrom(type))
                    return result;
                
                result = type.GetInterfaces()
                    .FirstOrDefault(i => !i.IsGenericParameter && i.IsAssignableFrom(type));
                
                if (result != null && type.IsAssignableFrom(result))
                    return result;
                
                throw new Exception();
            }
            
            return type;
        }

        static Regex reGA = new Regex(@"`\d+");

        static int varIndex = 0;

        static string ParameterToLine(ParameterInfo parameter) {
            string result;
            
            if (parameter.ParameterType.IsByRef) {
                if (parameter.IsOut)
                    return $"out var p{varIndex++}";
                else
                    result = "ref ";
            } else 
                result = "";
            
            result += $"({TypeToLine(parameter.ParameterType)}) p";
            return result;
        }
        
        static string TypeToLine(Type type) {
            if (type.IsArray)
                return TypeToLine(type.GetElementType()) + "[]";

            type = NoGenericParameterType(type);
            
            string result = "";

            Type declaringType = type;
            string dType = "";

            while (declaringType != null) {
                dType = declaringType.Name;

                Type[] genericArguments = declaringType.GetGenericArguments();
                if (genericArguments.Length > 0)
                    dType += $"<{string.Join(", ", genericArguments.Select(TypeToLine).ToArray())}>";

                result = dType + (result.Length > 0 ? "." : "") + result;

                declaringType = declaringType.DeclaringType;
            }

            // if (type.IsArray && type.FullName.Contains("+")) {
            //     var index = type.FullName.IndexOf('+');
            //     var outterType = type.Assembly.GetType(type.FullName.Substring(0, index));
            //     result = TypeToLine(outterType) + "." + result;
            // } else 
            if (type.Namespace != null) 
                result = type.Namespace + "." + result;

            return result;
        }

        #endregion
    }
}