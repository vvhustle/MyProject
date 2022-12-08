using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Yurowm.Extensions;
using Yurowm.Serialization;
using Yurowm.Utilities;
using Space = Yurowm.Spaces.Space;

namespace YMatchThree.Core {
    public class Gravitation : LevelSlotExtension {
        
        EachSlotGravitationVariable info;

        public override void OnAddToSpace(Space space) {
            base.OnAddToSpace(space);
            
            var slots = context.GetArgument<Slots>();
            
            foreach (Slot slot in slots.all.Values) { 
                slot.falling.Clear();
                if (info.directions.TryGetValue(slot.coordinate, out var direction)) {
                    foreach (Side side in Sides.straight)
                        if (direction.HasFlag(side))
                            slot.falling.Add(side, null);
                    slot.CalculateFallingSlot();
                }
            }
        }

        public override void SetupVariable(ISerializable variable) {
            base.SetupVariable(variable);
            switch (variable) {
                case EachSlotGravitationVariable esgv: info = esgv; return;
            }
        }

        public override IEnumerator GetVariblesTypes() {
            yield return base.GetVariblesTypes();
            yield return typeof(EachSlotGravitationVariable);
        }
    }
    
    public class EachSlotGravitationVariable : ContentInfoVariable, IFieldSensitiveVariable {
        public Dictionary<int2, Side> directions = new Dictionary<int2, Side>();

        public void MoveSlot(int2 from, int2 to) {
            if (directions.TryGetValue(from, out var value)) {
                directions[to] = value;
                directions.Remove(from);
            }
        }

        public void RemoveSlot(int2 from) {
            directions.Remove(from);
        }
        
        public override void Serialize(Writer writer) {
            writer.Write("directionKeys", directions.Keys.ToArray());
            writer.Write("directionValues", directions.Values.Cast<int>().ToArray());
        }

        public override void Deserialize(Reader reader) {
            var keys = reader.ReadCollection<int2>("directionKeys");
            var values = reader.ReadCollection<int>("directionValues");
            
            directions.Clear();
            directions.AddPairs(keys.Zip(values, (c, s) => new KeyValuePair<int2, Side>(c, (Side) s)));
        }
    }
}