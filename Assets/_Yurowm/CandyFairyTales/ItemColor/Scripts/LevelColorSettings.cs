using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Yurowm.Extensions;
using Yurowm.Serialization;
using Yurowm.Utilities;

namespace YMatchThree.Core {
    public abstract class LevelColorSettings : ISerializable {
        
        public ItemColorPalette colorPalette;
        public abstract int Count {get;}
        
        public Dictionary<int, int> dictionary = new Dictionary<int, int>();
        
        public int GetRandomColorID(YRandom random = null) {
            return dictionary.Values.Take(Count).GetRandom(random);
        }
        
        public int Convert(int colorID) {
            if (dictionary.TryGetValue(colorID, out var result))
                return result;
            return -1;
        }
        
        public virtual void Serialize(Writer writer) {
            if (colorPalette != null)
                writer.Write("palette", colorPalette?.ID);
        }

        public virtual void Deserialize(Reader reader) {
            var colorSetID = reader.Read<string>("palette");
            if (!colorSetID.IsNullOrEmpty())
                colorPalette = LevelContent.storage.Items<ItemColorPalette>()
                    .IDefaultOrFirst(s => s.ID == colorSetID);
        }
        
        public abstract void Initialize();
    }
}