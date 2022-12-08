using System;
using System.IO;
using System.Linq;
using UnityEngine;
using YMatchThree.Core;
using Yurowm.Extensions;
using Yurowm.Serialization;
using Yurowm.Utilities;

namespace YMatchThree.Editor {
    public class LevelEditorContext {
        public LevelEditorController controller;
        public LevelEditorBase editor;

        public readonly ILevelScriptPreset[] scriptPresets = Utils.FindInheritorTypes<ILevelScriptPreset>(true, false)
            .Where(t => !t.IsAbstract && !t.IsInterface)
            .Select(Activator.CreateInstance)
            .CastIfPossible<ILevelScriptPreset>()
            .ToArray();
        
        public LevelEditorFolders folders;
        
        public LevelEditorContext() {
            folders = new LevelEditorFolders(Path.Combine(directory.FullName, "folders.ys"));
        }

        #region Design
        
        public LevelScriptOrderedFile designFile {get; private set;}
        
        public LevelScriptOrdered design => designFile?.Script;
        public LevelDesignFileWorld currentWorld;
        
        #endregion
        
        static DirectoryInfo _directory = null;
        public static DirectoryInfo directory {
            get {
                if (_directory == null || !_directory.Exists) _directory = null;
                if (_directory == null)
                    _directory = new DirectoryInfo(Path.Combine(Application.streamingAssetsPath, nameof(LevelScriptOrdered)));
                if (!_directory.Exists)
                    _directory.Create();
                return _directory;
            }
        }
        
        public readonly Type[] colorSettingsTypes = Utils.FindInheritorTypes<LevelColorSettings>(true, false)
            .Where(t => !t.IsAbstract)
            .ToArray();

        public void SetDesign(LevelScriptOrderedFile designFile) {
            this.designFile = designFile;
        }

        public void SetDirty() {
            designFile?.SetDirty();
        }
    }
}
