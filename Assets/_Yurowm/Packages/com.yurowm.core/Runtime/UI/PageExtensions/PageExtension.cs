using Yurowm.Serialization;

namespace Yurowm.UI {
    public abstract class PageExtension : ISerializable {
        
        public virtual void OnShow() {}
        
        public virtual void Serialize(Writer writer) {}

        public virtual void Deserialize(Reader reader) {}
    }
}