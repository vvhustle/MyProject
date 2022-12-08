using System;
using System.Collections;
using System.Collections.Generic;
using Yurowm.Extensions;
using Yurowm.Serialization;

namespace YMatchThree.Core {
    [SlotTag(ConsoleColor.White, ConsoleColor.DarkGreen)]
    public class ChipGeneratorCharge : SlotContent {
        List<ContentInfo> chips = new List<ContentInfo>();
        
        public ContentInfo GetNext() {
            if (chips.Count == 0) {
                BreakParent();
                Kill();
                return null;
            }
            
            return chips.Grab();
        }
        
        public override IEnumerator GetVariblesTypes() {
            yield return base.GetVariblesTypes();
            yield return typeof(ChipGeneratorChargeVariable);
        }

        public override void SetupVariable(ISerializable variable) {
            base.SetupVariable(variable);
            switch (variable) {
                case ChipGeneratorChargeVariable c: chips = c.chips; return;
            }
        }

        public override Type GetContentBaseType() {
            return typeof(SlotModifier);
        }

        public override bool IsUniqueContent() {
            return false;
        }
    }
    
    public class ChipGeneratorChargeVariable : ContentInfoVariable {
        public List<ContentInfo> chips = new List<ContentInfo>();
        
        public override void Serialize(Writer writer) {
            writer.Write("chipsCharge", chips.ToArray());
        }

        public override void Deserialize(Reader reader) {
            chips.Clear();
            chips.AddRange(reader.ReadCollection<ContentInfo>("chipsCharge"));
        }
    }
}