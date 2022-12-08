using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Yurowm.Coroutines;
using Yurowm.Extensions;
using Yurowm.InUnityReporting;
using Yurowm.Utilities;

namespace Yurowm.Serialization {
    public class GameData : ISerializable {
        
        CryptKey Key;
        
        #region Modules

        List<Module> modules = new List<Module>();
        
        public M GetModule<M>() where M : Module {
            return modules.CastOne<M>() ?? CreateModule<M>();
        }
        
        public bool CreateModule<M>(out M module) where M : Module {
            module = CreateModule<M>();
            return module != null;
        }
        
        public M CreateModule<M>() where M : Module {
            var result = modules.CastOne<M>();
            
            if (result != null)
                return null;

            try {
                result = Activator.CreateInstance<M>();
                result.setDirty = SetDirty;
                result.Initialize();
                modules.Add(result);
            } catch (Exception e) {
                Debug.LogException(e);
                result = null;
            }
            
            return result;
        }
        
        public abstract class Module : ISerializable {
            public Action setDirty;
        
            public void SetDirty() {
                setDirty.Invoke();    
            }
        
            public virtual void Initialize() {}
    
            public virtual string GetKey() => GetType().Name.ToLowerInvariant();
                
            public abstract void Serialize(Writer writer);

            public abstract void Deserialize(Reader reader);
        }
        
        #endregion
        
        readonly string fileName;
        readonly string backupFileFormat = "{0}_backup" + Serializator.FileExtension;
        
        public GameData(string name, string cryptKey = null) {
            fileName = name + Serializator.FileExtension;
            Key = cryptKey.IsNullOrEmpty() ? null : CryptKey.Get(cryptKey);
        }
        
        #region Save & Load
        
        bool isDirty;
        
        public void SetDirty() {
            isDirty = true;
        }

        IEnumerator dirtyCheck;
        
        IEnumerator DirtyCheck() {
            var thisProcess = dirtyCheck;
            while (thisProcess == dirtyCheck) {
                if (isDirty) {
                    Save(fileName);
                    yield return new Wait(1);
                }
                yield return null;
            }
        }
        
        IEnumerator ReadFromFile(string fileName) {
            string raw = null;
            
            yield return TextData.LoadTextProcess(
                Path.Combine("Data", fileName),
                r => raw = r,
                TextCatalog.Persistent);
            
            if (!raw.IsNullOrEmpty()) {
                if (Key != null)
                    raw = raw.Decrypt(Key);
                
                Serializator.FromTextData(this, raw);
            }
            
            Initialize();
        }

        void Save(string fileName) {
            isDirty = false;
            
            string raw = Serializator.ToTextData(this);
            
            if (Key != null)
                raw = raw.Encrypt(Key);
            
            TextData.SaveText(
                Path.Combine("Data", fileName),
                raw,
                TextCatalog.Persistent);
        }
        
        
        public void Backup(string name) {
            Save(backupFileFormat.FormatText(name));
        }
        
        
        public void Restore(string name) {
            ReadFromFile(backupFileFormat.FormatText(name)).Run();
        }

        public IEnumerator Load() {
            yield return ReadFromFile(fileName);
        }
        
        void Initialize() {
            modules.ForEach(m => m.Initialize());
            dirtyCheck = DirtyCheck();
            dirtyCheck.Run();
            Reporter.AddReport($"Game Data ({fileName})", new SerializableReport(this));
        }
        
        public void Clear() {
            modules.Clear();
            SetDirty();
        }   
        
        #endregion
        
        #region ISerializable
        
        public void Serialize(Writer writer) {
            writer.Write("modules", modules.ToArray());
        }

        public void Deserialize(Reader reader) {
            modules.Clear();
            modules.AddRange(reader.ReadCollection<Module>("modules"));
            modules.ForEach(m => m.setDirty = SetDirty);
        }
        
        #endregion
    }
}