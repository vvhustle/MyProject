using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Yurowm.GUIHelpers;
using Yurowm.Serialization;

namespace Yurowm.Sounds {
    public static class SoundEditorHelpers {
        public static DirectoryInfo GetRootFolder() {
            return new DirectoryInfo(Path.Combine(TextData.GetPath(TextCatalog.StreamingAssets), SoundController.RootFolderName));
        }
        
        public static IEnumerable<string> GetAllSoundPaths() {
            
            IEnumerable<FileInfo> GetAllFiles(DirectoryInfo directory) {
                foreach (var file in directory.EnumerateFiles()) {
                    yield return file;
                }    
                
                foreach (var dir in directory.EnumerateDirectories()) 
                    foreach (var file in GetAllFiles(dir))
                        yield return file;
            }
            
            var rootDirectory = GetRootFolder();
            
            var trimSize = rootDirectory.FullName.Length + 1;
            
            foreach (var file in GetAllFiles(rootDirectory)) {
                switch (file.Extension) {
                    case ".mp3": break;
                    case ".wav": break;
                    case ".ogg": break;
                    default: continue;
                }
                
                yield return file.FullName.Substring(trimSize).Replace('\\', '/');
            }
        }
        
        public static void EditClipPath(string label, string path, Action<string> onChange) {
            using (GUIHelper.Horizontal.Start()) {
        
                using (GUIHelper.Change.Start(() => onChange(path))) 
                    path = EditorGUILayout.TextField(label, path);

                if (GUILayout.Button("<", GUILayout.Width(20))) {
                    var menu = new GenericMenu();

                    foreach (var p in SoundEditorHelpers.GetAllSoundPaths()) {
                        var _p = p;
                        menu.AddItem(new GUIContent(p), path == p, () => onChange(_p));
                    }

                    if (menu.GetItemCount() > 0)
                        menu.ShowAsContext();
                    
                }
            }
        }
    }
}