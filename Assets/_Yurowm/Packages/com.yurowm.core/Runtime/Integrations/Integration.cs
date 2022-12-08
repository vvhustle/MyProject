using System;
using System.Linq;
using UnityEngine;
using Yurowm.Extensions;
using Yurowm.Serialization;
using Yurowm.Utilities;

namespace Yurowm.Integrations {
    public abstract class Integration : ISerializable, IStorageElementExtraData {
        
        [PreloadStorage]
        public static Storage<Integration> storage = new Storage<Integration>("Integrations", TextCatalog.StreamingAssets, true);
        
        public StorageElementFlags storageElementFlags { get; set; }

        public const int INITIALIZE_ORDER = -900;
        
        [OnLaunch(INITIALIZE_ORDER)]
        public static void InitializeOnLoad() {
            if (OnceAccess.GetAccess("Integration"))
                storage
                    .Where(i => i.active && i.HasAllNecessarySDK())
                    .ForEach(i => {
                        try {
                            i.Initialize();
                        } catch (Exception e) {
                            Debug.LogException(e);
                        }
                    });
        }
        
        public bool active = true;
        
        public virtual void Initialize() {}

        public abstract string GetName();

        public static I Get<I>() where I : Integration {
            return storage.Items<I>().FirstOrDefault(i => i.active && i.HasAllNecessarySDK());
        }
        
        #region ISerializable

        public virtual void Serialize(Writer writer) {
            writer.Write("active", active);
            writer.Write("storageElementFlags", storageElementFlags);
        }

        public virtual void Deserialize(Reader reader) {
            reader.Read("active", ref active);
            storageElementFlags = reader.Read<StorageElementFlags>("storageElementFlags");
        }
        
        #endregion

        public virtual bool HasAllNecessarySDK() {
            return true;
        }
    }
}