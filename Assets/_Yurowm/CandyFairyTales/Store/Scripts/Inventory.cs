using System.Collections.Generic;
using Yurowm.Extensions;
using Yurowm.Serialization;

namespace Yurowm.Store {
    public class Inventory : GameData.Module {
        Dictionary<string, int> counts = new Dictionary<string, int>();
        
        public override void Serialize(Writer writer) {
            writer.Write("counts", counts);
        }

        public override void Deserialize(Reader reader) {
            counts.Reuse(reader.ReadDictionary<int>("counts"));
        }

        public int GetItemCount(string itemID) {
            return counts.Get(itemID);
        }

        public void EarnItems(string itemID, int count) {
            if (count <= 0) return;
            counts[itemID] = counts.Get(itemID) + count;
            SetDirty();
        }

        public void SpendItems(string itemID, int count) {
            if (count <= 0) return;
            counts[itemID] = (counts.Get(itemID) - count).ClampMin(0);
            SetDirty();
        }
    }
}