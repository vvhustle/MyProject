using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yurowm.Serialization;

namespace YMatchThree.Core {
    public class FixedSizeColorSettings : LevelColorSettings {
        public int count = 4;
        
        public override int Count => count;

        public override void Initialize() {
            count = Mathf.Min(count, colorPalette.colors.Count, ItemColorInfo.IDs.Length);
            
            dictionary.Clear();
            for (int index = 0; index < count; index++) {
                var id = ItemColorInfo.IDs[index];
                dictionary.Add(id, id);
            }
        }

        #region ISerializable


        public override void Serialize(Writer writer) {
            base.Serialize(writer);
            writer.Write("count", count);
        }

        public override void Deserialize(Reader reader) {
            base.Deserialize(reader);
            reader.Read("count", ref count);
        }

        #endregion
    }
}