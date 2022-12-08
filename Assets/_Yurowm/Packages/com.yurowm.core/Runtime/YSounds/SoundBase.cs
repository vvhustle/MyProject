using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yurowm.Extensions;
using Yurowm.Serialization;

namespace Yurowm.Sounds {
    public abstract class SoundBase : ISerializableID {
        
        [PreloadStorage]
        public static Storage<SoundBase> storage = new Storage<SoundBase>("Sounds", TextCatalog.StreamingAssets, true);

        public abstract void Play();
        
        public string ID { get; set; }
        
        public virtual void Serialize(Writer writer) {
            writer.Write("ID", ID);
        }

        public virtual void Deserialize(Reader reader) {
            ID = reader.Read<string>("ID");
        }
    }
}