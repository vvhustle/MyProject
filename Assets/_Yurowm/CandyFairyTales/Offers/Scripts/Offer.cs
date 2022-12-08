using Yurowm.Serialization;

namespace Yurowm.Offers {
    public abstract class Offer : ISerializableID {
        
        [PreloadStorage]
        public static Storage<Offer> storage = new Storage<Offer>("Offers", TextCatalog.StreamingAssets, true);
        
        public string ID { get; set; }

        public virtual void Serialize(Writer writer) { 
            writer.Write("ID", ID);
        }

        public virtual void Deserialize(Reader reader) {
            ID = reader.Read<string>("ID");
        }

        public abstract void Show();
    }
}