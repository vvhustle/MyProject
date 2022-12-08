using System.Collections;
using UnityEngine;
using Yurowm.Coroutines;
using Yurowm.Extensions;
using Yurowm.Serialization;
using Yurowm.Utilities;

namespace YMatchThree.Core {
    public class Teleport : SlotModifier {
        
        Slot targetSlot;
        int2 targetCoord;

        public override void OnChangeSlot() {
            base.OnChangeSlot();
            
            targetSlot = field.slots.all.Get(targetCoord);
            if (targetSlot == null) return;
            
            slotModule.Slot().onAddContent += TryToTeleport;
            targetSlot.onRemoveContent += TryToTeleport;
            
            Logic().Run(field.coroutine);
        }

        bool requested = false;
        
        IEnumerator Logic() {
            var sourceSlot = slotModule.Slot();
            
            while (IsAlive()) {
                
                if (requested) {
                    var content = sourceSlot.GetCurrentContent();
                    if (content is Chip chip && !targetSlot.HasBaseContent<Chip>() && !targetSlot.HasBaseContent<Block>()) {
                        chip.toLand = true;
                        
                        while (chip.localPosition.MagnitudeIsGreaterThan(Slot.Offset * .2f))
                            yield return null;

                        targetSlot.AddContent(chip);
                        
                        chip.localPosition = Vector2.zero;
                        
                        chip.Show();

                    }
                    
                    requested = false;
                }
                
                yield return null;
            }
        }

        void TryToTeleport(SlotContent _) {
            requested = true;
        }

        public override IEnumerator GetVariblesTypes() {
            yield return base.GetVariblesTypes();
            yield return typeof(CoordinateVariable);
        }

        public override void SetupVariable(ISerializable variable) {
            base.SetupVariable(variable);
            switch (variable) {
                case CoordinateVariable c: {
                    targetCoord = c.value;
                    return;
                }
            }
        }
    }
    
    public class CoordinateVariable : ContentInfoVariable, IFieldSensitiveVariable {
        public int2 value = int2.Null;

        public void MoveSlot(int2 from, int2 to) {
            if (from == value)
                value = to;
        }

        public void RemoveSlot(int2 from) {
            if (from == value)
                value = int2.Null;
        }
        
        public override void Serialize(Writer writer) {
            writer.Write("value", value);
        }

        public override void Deserialize(Reader reader) {
            reader.Read("value", ref value);
        }
    }
}