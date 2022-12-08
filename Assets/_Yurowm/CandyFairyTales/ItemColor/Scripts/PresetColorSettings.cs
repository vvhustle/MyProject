using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Yurowm.Serialization;

namespace YMatchThree.Core {
    public class PresetColorSettings : LevelColorSettings {
        public List<int> colorIDs = ItemColorInfo.IDs.Take(4).ToList();
        
        public override int Count => colorIDs.Count;
        
        public override void Initialize() {
            var count = Mathf.Min(Count, colorPalette.colors.Count, ItemColorInfo.IDs.Length, colorIDs.Count);
            
            dictionary.Clear();
            for (int index = 0; index < count; index++)
                dictionary.Add(ItemColorInfo.IDs[index], colorIDs[index]);
        }

        #region ISerializable


        public override void Serialize(Writer writer) {
            base.Serialize(writer);
            writer.Write("IDs", colorIDs.ToArray());
        }

        public override void Deserialize(Reader reader) {
            base.Deserialize(reader);
            colorIDs.Clear();
            colorIDs.AddRange(reader.ReadCollection<int>("IDs"));
        }

        #endregion
        
    }
}