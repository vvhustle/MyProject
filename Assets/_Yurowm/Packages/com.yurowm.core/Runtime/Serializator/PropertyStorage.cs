using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Yurowm.Extensions;
using Yurowm.Utilities;

namespace Yurowm.Serialization {
    public static class PropertyStorage {
        
        [OnLaunch(int.MinValue)]
        static IEnumerator OnLaunch() {
            if (OnceAccess.GetAccess("PropertyStorage")) {
                if (TextData.HasFastLoadingSupport())
                    yield break;
                
                var storages = Utils
                    .FindInheritorTypes<IPropertyStorage>(true, true)
                    .Where(t => !t.IsAbstract && !t.IsInterface)
                    .Select(Activator.CreateInstance)
                    .CastIfPossible<IPropertyStorage>();

                foreach (var storage in storages)
                    yield return LoadProcess(storage);
            }
        }
        
        static readonly CryptKey KEY = CryptKey.Get("sdBjVihwQkyuvxYrkAWY");
        
        public static void Save(IPropertyStorage storage) {
            string raw = Serializator.ToTextData(storage, true);
            if (storage.Encrypted)
                raw = raw.Encrypt(KEY);
            TextData.SaveText(Path.Combine("Data", storage.FileName), raw, storage.Catalog);
        }

        static void Load(IPropertyStorage storage, string raw) {
            if (raw.IsNullOrEmpty())
                return;
            
            if (storage.Encrypted)
                raw = raw.Decrypt(KEY);
                
            Serializator.FromTextData(storage, raw);
        }

        public static void Load(IPropertyStorage storage) {
            Load(storage, TextData.LoadText(Path.Combine("Data", storage.FileName), storage.Catalog));
        }

        static IEnumerator LoadProcess(IPropertyStorage storage) {
            yield return TextData.LoadTextProcess(Path.Combine("Data", storage.FileName),
                raw => Load(storage, raw),
                storage.Catalog);
        }

        static Dictionary<Type, IPropertyStorage> loadedStorages = new Dictionary<Type, IPropertyStorage>();
        
        public static S Load<S>() where S : IPropertyStorage {
            if (loadedStorages.TryGetValue(typeof(S), out var storage))
                return (S) storage;
            try {
                var result = Activator.CreateInstance<S>();
                Load(result);
                loadedStorages.Add(typeof(S), result);
                return result;
            } catch (Exception e) {
                Debug.LogException(e);
            }
            return default;
        }
    }
    
    public interface IPropertyStorage : ISerializable {
        string FileName {get;}
        TextCatalog Catalog {get;}
        bool Encrypted {get;}
    } 
}