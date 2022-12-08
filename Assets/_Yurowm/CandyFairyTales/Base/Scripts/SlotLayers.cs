using System.Collections.Generic;
using System.Linq;
using Yurowm.Serialization;
using Yurowm.Utilities;

namespace YMatchThree.Core {
    public abstract class SlotLayerBase : ISerializable {
        public string ID;
        public bool isDefault;
        
        public virtual void Initialize(Level level) {}
        
        public abstract bool Contains(Level level, SlotInfo slot);
        public abstract bool Contains(Level level, int2 coord);
        
        public IEnumerable<SlotInfo> GetSlots(Level level) {
            foreach (var slot in level.slots)
                if (Contains(level, slot))
                    yield return slot;
        }

        public virtual void Serialize(Writer writer) {
            writer.Write("ID", ID);
            if (isDefault)
                writer.Write("default", isDefault);
        }

        public virtual void Deserialize(Reader reader) {
            reader.Read("ID", ref ID);
            reader.Read("default", ref isDefault);
        }
    }
    
    public class AllSlotsLayer : SlotLayerBase {
        public override bool Contains(Level level, SlotInfo slot) {
            return Contains(level, slot.coordinate);
        }

        public override bool Contains(Level level, int2 coord) {
            return coord.X >= 0 && coord.Y >= 0 &&
                   coord.X < level.width && coord.Y < level.height;
            
        }
    }

    public class SlotLayer : SlotLayerBase {
        public SlotInfo[] slots;
        
        int2[] coordinates;

        public override void Initialize(Level level) {
            base.Initialize(level);
            slots = level.slots.Where(s => coordinates.Contains(s.coordinate)).ToArray();
            coordinates = null;
        }

        public override bool Contains(Level level, SlotInfo slot) {
            return slots.Contains(slot);
        }

        public override bool Contains(Level level, int2 coord) {
            return slots.Any(s => s.coordinate == coord);
        }

        public void SetSelection(IEnumerable<SlotInfo> slots) {
            this.slots = slots.ToArray();
        }
        
        public override void Serialize(Writer writer) {
            base.Serialize(writer);
            writer.Write("coordinates", slots.Select(s => s.coordinate).ToArray());
        }

        public override void Deserialize(Reader reader) {
            base.Deserialize(reader);
            coordinates = reader.ReadCollection<int2>("coordinates").ToArray();
        }
    }
}