using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Yurowm.Coroutines;
using Yurowm.DebugTools;
using Yurowm.Extensions;
using Yurowm.Utilities;

namespace Yurowm.Serialization {

    public abstract class Storage : ISerializable {
        
        static readonly CryptKey KEY = CryptKey.Get("Xq3zWBlMu3y3B6hRcJe3");
        
        [OnLaunch(int.MinValue)]
        static IEnumerator OnLaunch() {
            if (OnceAccess.GetAccess("Storage")) {
                if (TextData.HasFastLoadingSupport())
                    yield break;

                foreach (var field in Utils
                    .GetAllFieldsWithAttribute<PreloadStorageAttribute>(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                    .Select(t => t.Item1)
                    .Cast<FieldInfo>())
                    if (field.GetValue(null) is Storage storage)
                        yield return storage.Load();
            }
        }
        
        bool isLoaded = false;
        public bool IsLoaded => isLoaded;
        readonly bool encrypted;

        protected string fileName;
        public string Name => fileName;
        
        TextCatalog catalog;
        
        public Storage() { }
        
        public Storage(string fileName, TextCatalog catalog, bool encrypted = false) {
            this.encrypted = encrypted;
            this.fileName = fileName + Serializator.FileExtension;
            this.catalog = catalog; 
            if (TextData.HasFastLoadingSupport()) 
                LoadFast();
        }

        public void Apply() {
            if (!IsLoaded) 
                LoadFast();
            string raw = Serializator.ToTextData(this, true);
            if (encrypted)
                raw = raw.Encrypt(KEY);
            TextData.SaveText(Path.Combine("Data", fileName), raw, catalog);
        }
        
        public IEnumerator GetSource(Action<string> getResult) {
            if (getResult == null)
                yield break;

            string source = null;
            
            if (TextData.HasFastLoadingSupport())
                source = TextData.LoadText(Path.Combine("Data", fileName), catalog);
            else
                yield return TextData.LoadTextProcess(Path.Combine("Data", fileName), r => source = r, catalog);
            
            if (!source.IsNullOrEmpty() && encrypted) 
                source = source.Decrypt(KEY);
            
            getResult.Invoke(source);
        }
        
        
        public void LoadFast() {
            Load().Complete().Run();
        }

        protected virtual IEnumerator Load() {
            isLoaded = true;
            string source = null;
            
            yield return GetSource(r => source = r);
            
            if (source.IsNullOrEmpty()) 
                yield break;
            
            try {
                Serializator.FromTextData(this, source);
            } catch (Exception e) {
                Debug.LogException(e);
            }
        }
        
        public abstract void Serialize(Writer writer);

        public abstract void Deserialize(Reader reader);

        [JITMethodIssueTypeProvider]
        static IEnumerator<Type> GetJITMITypes() {
            yield return typeof(ISerializable);
            yield return typeof(IValuePacker);
        }
    }
    
    public class Storage<S> : Storage, IEnumerable<S> where S : ISerializable {

        List<S> _items = new List<S>();
        
        public List<S> items {
            get {
                if (!IsLoaded)
                    LoadFast();
                return _items;
            }
        }
        
        public Storage(string fileName, TextCatalog catalog, bool encrypted = false) :
            base(fileName, catalog, encrypted) {
            Initialize();
        }
        
        public Storage() : base() {
            Initialize();
        }
        
        bool hasIDs;

        void Initialize() {
            hasIDs = typeof(ISerializableID).IsAssignableFrom(typeof(S));
        }
        
        protected override IEnumerator Load() {
            yield return base.Load();
            
            if (!Application.isEditor) {
                if (UnityEngine.Debug.isDebugBuild)
                    items.RemoveAll(i => i is IStorageElementExtraData e && e.storageElementFlags.HasFlag(StorageElementFlags.ReleaseOnly));
                else
                    items.RemoveAll(i => i is IStorageElementExtraData e && e.storageElementFlags.HasFlag(StorageElementFlags.DebugOnly));
            }
                
            if (filter != null)
                items.RemoveAll(i => !filter(i));
                
            if (typeof(IComparable).IsAssignableFrom(typeof(S)) || typeof(IComparable<S>).IsAssignableFrom(typeof(S)))
                items.Sort();
        }
        
        Func<S, bool> filter;
        
        public void SetLoadFilter(Func<S, bool> filter) {
            this.filter = filter;
            if (IsLoaded && filter != null)
                items.RemoveAll(i => !filter(i));
        }

        public static Storage<S> Load(string fileName, TextCatalog catalog) {
            return new Storage<S>(fileName, catalog);
        }

        public IEnumerable<T> Items<T>() where T : S {
            if (!IsLoaded) LoadFast();
            return _items.CastIfPossible<T>();
        }  
        
        public T GetDefault<T>() where T : S, IStorageElementExtraData {
            return Items<T>()
                .FirstOrDefaultFiltered(
                    t => t.storageElementFlags.HasFlag(StorageElementFlags.DefaultElement),
                    t => true);
        } 

        public IEnumerable<T> GetAllDefault<T>() where T : S, IStorageElementExtraData {
            return Items<T>()
                .Where(t => t.storageElementFlags.HasFlag(StorageElementFlags.DefaultElement));
        }

        public T GetItem<T>(Func<T, bool> filter = null) where T : S {
            if (!IsLoaded) LoadFast();
            if (filter == null)
                return _items.CastOne<T>();
            return _items.CastIfPossible<T>().FirstOrDefault(filter);
        }
        
        public T GetItemByID<T>(string ID) where T : S {
            if (hasIDs)
                return (T) items
                    .CastIfPossible<T>()
                    .Cast<ISerializableID>()
                    .FirstOrDefault(isid => isid.ID == ID);
         
            return default;
        }
        
        public S GetItemByID(string ID) {
            return GetItemByID<S>(ID);
        }
        
        #region ISerializable
        public override void Deserialize(Reader reader) {
            _items = reader.ReadCollection<S>("items")
                .Where(i => i != null)
                .ToList();
        }

        public override void Serialize(Writer writer) {
            writer.Write("items", _items);
        }
        #endregion

        #region IEnumerable<S>
        public IEnumerator<S> GetEnumerator() {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
        #endregion
    }
    
    public class PreloadStorageAttribute : Attribute {}
    
    [Flags]
    public enum StorageElementFlags {
        DefaultElement = 1 << 1,
        WorkInProgress = 1 << 2,
        DebugOnly = 1 << 3,
        ReleaseOnly = 1 << 4
    }
    
    public interface IStorageElementExtraData {
        StorageElementFlags storageElementFlags {get; set;}
    }
    
    public interface ISerializableID : ISerializable {
        string ID {get; set;}
    }
}