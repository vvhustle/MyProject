using System;
using System.IO;
using System.Linq;
using UnityEngine;
using YMatchThree.Editor;
using Yurowm.Coroutines;
using Yurowm.Extensions;
using Yurowm.Serialization;
using Yurowm.Utilities;

namespace YMatchThree.Core {
    
    public abstract class LevelScriptFile {
        public FileInfo file { get; protected set; }
        
        public LevelScriptFile(FileInfo file) {
            this.file = file;
        }
        
        internal bool dirty {get; set;}
        
        public void SetDirty() {
            dirty = true;
        }
    }
    
    public class LevelScriptFile<S> : LevelScriptFile where S : LevelScriptBase {

        
        protected S script = null;
        
        public S ScriptPreview {
            get {
                if (!IsLoaded()) 
                    Load(true);
                return script;
            }
        }
        
        public S Script {
            get {
                if (!IsLoaded() || script.IsPreview) 
                    Load(false);
                return script;
            }
        }

        public LevelScriptFile(FileInfo file) : base(file) {
            this.file = file;
        }

        public LevelScriptFile(S script, string key) : base(null) {
            this.script = script;
            script.ID = key;
            file = new FileInfo(Path.Combine(Application.streamingAssetsPath, 
                script.GetType().Name,
                LevelScriptBase.fileNameFormat.FormatText(key)));
        }

        public bool IsLoaded() {
            return script != null;
        }
        
        #region Serialization
        
        public void Save() {
            if (!IsLoaded()) return;
            
            string raw = Serializator.ToTextData(script);
            if (!file.Directory.Exists) file.Directory.Create();
            File.WriteAllText(file.FullName, raw);
            
            dirty = false;
        }
        
        string raw = null;

        bool preview;
        
        public void Load(bool preview) {
            if (!file.Exists) return;
            
            if (raw.IsNullOrEmpty())
                raw = File.ReadAllText(file.FullName);
            
            script = LevelScriptBase.Load<S>(raw, preview);
            
            if (script.ID.IsNullOrEmpty()) {
                script.LoadCompletely().Complete().Run();
                try {
                    FileInfo newFile;
                    while (true) {
                        script.ID = LevelScriptEditor.NewID();
                        newFile = new FileInfo(Path.Combine(Application.streamingAssetsPath, 
                            script.GetType().Name,
                            LevelScriptBase.fileNameFormat.FormatText(script.ID)));
                        if (!newFile.Exists) break;
                    }
                    File.Move(file.FullName, newFile.FullName);
                    file = newFile;
                    dirty = true;
                    Save();
                }
                catch (Exception e) {
                    Debug.LogException(e);
                }
            }
        }
        
        #endregion

        public override bool Equals(object obj) {
            return obj is LevelScriptFile<S> levelFile && file.Equals(levelFile.file);
        }

        public override int GetHashCode() {
            return file.GetHashCode();
        }

        public override string ToString() {
            return script.ToString();
        }
    }
}