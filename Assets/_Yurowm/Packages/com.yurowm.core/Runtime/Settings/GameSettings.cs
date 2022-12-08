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
    public class GameSettings : ISerializable {
        
        CryptKey KEY;
        
        #region Modules

        List<SettingsModule> modules = new List<SettingsModule>();
        
        public M GetModule<M>() where M : SettingsModule {
            M result = modules.CastOne<M>();
            if (result == null) {
                try {
                    result = Activator.CreateInstance<M>();
                    result.setDirty = SetDirty;
                    result.Initialize();
                    modules.Add(result);
                } catch (Exception e) {
                    Debug.LogException(e);
                    return null;
                }
            }
        
            return result;
        }
        
        #endregion
        
        #region Dictionary

        Dictionary<string, string> dictionary = new Dictionary<string, string>();
        
        public string Get(string key) {
            if (key.IsNullOrEmpty()) return null;
            return dictionary.Get(key);
        }
        
        public void Set(string key, string value) {
            dictionary.Set(key, value);
            isDirty = true;
        }

        #endregion
        
        public void Initialize() {
            modules.ForEach(m => m.Initialize());
            DirtyCheck().Run();
            Reporter.AddReport("Game Settings", new SerializableReport(this));
        }

        #region Save & Load
        
        bool isDirty = false;
        
        public void SetDirty() {
            isDirty = true;
        }

        IEnumerator DirtyCheck() {
            while (true) {
                if (isDirty) 
                    Save();
                yield return null;
            }
        }

        public void Save() {
            string raw = Serializator.ToTextData(this);
            if (KEY != null)
                raw = raw.Encrypt(KEY);
            
            TextData.SaveText(
                Path.Combine("Data", "GameSettings" + Serializator.FileExtension),
                raw,
                TextCatalog.Persistent);
        }
        
        public static GameSettings Instance {
            get;
            private set;
        }
        
        public static IEnumerator Load(string cryptKey = null) {
            if (Instance != null) 
                yield break;

            Instance = new GameSettings();
            
            CryptKey key = null;
            
            if (cryptKey.IsNullOrEmpty())
                Debug.LogError("You are trying to load settings without cryption key");
            else
                key = CryptKey.Get(cryptKey);
            
            string raw = null;
            yield return TextData.LoadTextProcess(
                Path.Combine("Data", "GameSettings" + Serializator.FileExtension),
                r => raw = r,
                TextCatalog.Persistent);

            if (!raw.IsNullOrEmpty()) {
                if (key != null)
                    raw = raw.Decrypt(key);
                
                Serializator.FromTextData(Instance, raw);
            }
            
            Instance.KEY = key;
            
            Instance.Initialize();
        }
        
        public void Clear() {
            dictionary.Clear();
            modules.Clear();
            SetDirty();
        }   
        
        #endregion
        
        #region ISerializable
        
        public void Serialize(Writer writer) {
            writer.Write("modules", modules.ToArray());
            writer.Write("dictionary", dictionary);
        }

        public void Deserialize(Reader reader) {
            modules.Clear();
            modules.AddRange(reader.ReadCollection<SettingsModule>("modules"));
            modules.ForEach(m => m.setDirty = SetDirty);
            dictionary = reader.ReadDictionary<string>("dictionary").ToDictionary();
        }
        
        #endregion
    }
    
    public abstract class SettingsModule : ISerializable {
        public Action setDirty;
        
        public void SetDirty() {
            setDirty.Invoke();    
        }
        
        public virtual void Initialize() {}
    
        public abstract void Serialize(Writer writer);

        public abstract void Deserialize(Reader reader);
    }
}