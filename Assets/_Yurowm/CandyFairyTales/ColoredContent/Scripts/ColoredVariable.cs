using Yurowm;
using Yurowm.Serialization;

namespace YMatchThree.Core {
    public class ColoredVariable : ContentInfoVariable {
        public ItemColorInfo info { get; set; } = ItemColorInfo.Unknown;
        
        public ColoredVariable() {}
    
        public override void Serialize(Writer writer) {
            writer.Write("info", info);
        }

        public override void Deserialize(Reader reader) {
            info = reader.Read<ItemColorInfo>("info");
        }
    }
}