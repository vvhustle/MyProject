using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using Yurowm.Console;
using Yurowm.Coroutines;
using Yurowm.Extensions;

namespace Yurowm.Serialization {
    public static class TextData {
        
        public static bool HasFastLoadingSupport() {
            #if UNITY_EDITOR
            return true;
            #endif
            #if UNITY_WEBGL
            return false;
            #endif
            return true;
        }
        
        static bool HasReadAccess(TextCatalog catalog) {
            #if UNITY_EDITOR
            return true;
            #else
            return catalog != TextCatalog.EditorDefaultResources && catalog != TextCatalog.ProjectFolder;
            #endif
        }
        
        static bool HasWriteAccess(TextCatalog catalog) {
            #if UNITY_EDITOR
            return true;
            #if UNITY_WEBGL
            return false;
            #endif
            #else
            return catalog == TextCatalog.Persistent;
            #endif
        }
        
        public static string GetPath(TextCatalog catalog) {
            switch (catalog) {
                case TextCatalog.StreamingAssets: return Application.streamingAssetsPath;
                case TextCatalog.Persistent: return Application.persistentDataPath;
                case TextCatalog.ProjectFolder: return Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Storage");
                case TextCatalog.EditorDefaultResources: return Path.Combine(Application.dataPath, "Editor Default Resources");
            }
            
            Debug.LogError("Unknown type of catalog");
            return null;
        }
        
        public static void SaveText(string path, string text, TextCatalog catalog = TextCatalog.StreamingAssets) {
            if (!HasWriteAccess(catalog))
                throw new Exception("No write access for the catalog: " + catalog);
            
            var fullPath = GetPath(catalog);
            
            if (fullPath.IsNullOrEmpty())
                return;
            
            fullPath = Path.Combine(fullPath, path);
            
            var file = new FileInfo(fullPath);
            
            if (!file.Directory.Exists)
                file.Directory.Create();
            
            File.WriteAllText(file.FullName, text);
        }
        
        public static void RemoveText(string path, TextCatalog catalog = TextCatalog.StreamingAssets) {
            if (!HasWriteAccess(catalog))
                throw new Exception("No write access for the catalog: " + catalog);
            
            
            var fullPath = GetPath(catalog);
            
            if (fullPath.IsNullOrEmpty())
                return;
            
            fullPath = Path.Combine(fullPath, path);

            var file = new FileInfo(fullPath);

            if (file.Exists) 
                File.Delete(file.FullName);
        }
        
        [QuickCommand("loadtext", "Data/Pages.ys", "Load StreamingAssets/Data/Pages.ys file and show text")]
        static IEnumerator LoadTextCommand(string path) {
            string result = null;
            yield return LoadTextProcess(path, r => result = r);
            
            if (result.IsNullOrEmpty())
                yield return YConsole.Error("The file is empty or doesn't exist");
            else
                yield return YConsole.Alias(result);
        }
        
        static string GetFullPath(string path, TextCatalog catalog) {
            if (!HasReadAccess(catalog))
                throw new Exception("No read access for the catalog: " + catalog);

            var fullPath = GetPath(catalog);
            
            if (fullPath.IsNullOrEmpty())
                return null;
            
            return Path.Combine(fullPath, path);
        }
        
        static IEnumerator LoadTextFromRequest(string url, bool fast, Action<string> getResult) {
            using (var request = UnityWebRequest.Get(url)) {
                var operation = request.SendWebRequest();

                if (fast) {
                    while (operation.webRequest.result == UnityWebRequest.Result.InProgress)
                        System.Threading.Thread.Sleep(3);
                } else 
                    yield return operation;

                switch (request.result) {
                    case UnityWebRequest.Result.Success:
                        getResult.Invoke(request.downloadHandler.text);
                        yield break;
                    case UnityWebRequest.Result.ConnectionError:
                    case UnityWebRequest.Result.ProtocolError:
                    case UnityWebRequest.Result.DataProcessingError: {
                        throw new Exception($"Text is not loaded.\n{request.result}: {request.error}\n{url}");
                    }
                    case UnityWebRequest.Result.InProgress:
                        throw new Exception($"Request result is in progress.\n{url}");
                }

                getResult.Invoke(null);
            }
        }
        
        static string LoadTextFromFile(string path) {
            if (!File.Exists(path))
                return null;
            
            return File.ReadAllText(path);
        }
        
        public static string LoadText(string path, TextCatalog catalog = TextCatalog.StreamingAssets) {
            if (!HasFastLoadingSupport())
                throw new Exception("It is not supported");
            
            path = GetFullPath(path, catalog);
                
            string result = null;

            if (!Application.isEditor && path.Contains("://")) {
                LoadTextFromRequest(path, true, r => result = r).Run();
            } else {
                result = LoadTextFromFile(path);
            }
            
            return result;
        }
        
        public static IEnumerator LoadTextProcess(string path, Action<string> getResult, TextCatalog catalog = TextCatalog.StreamingAssets) {
            path = GetFullPath(path, catalog);
                
            if (!Application.isEditor && path.Contains("://"))
                yield return LoadTextFromRequest(path, false, getResult);
            else
                getResult.Invoke(LoadTextFromFile(path));
        }
    }
    
    public enum TextCatalog {
        StreamingAssets,
        EditorDefaultResources,
        Persistent,
        ProjectFolder
    }
}
