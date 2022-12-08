using Yurowm.Serialization;

namespace Yurowm.Advertising {
    public class AdKey : ISerializable {
        public string key;
        public string placeID;
        public AdPlatform platform;
        public AdType type;
        
        public void Serialize(Writer writer) {
            writer.Write("key", key);
            writer.Write("placeID", placeID);
            writer.Write("platform", platform);
            writer.Write("adType", type);
        }

        public void Deserialize(Reader reader) {
            reader.Read("key", ref key);
            reader.Read("placeID", ref placeID);
            reader.Read("platform", ref platform);
            reader.Read("adType", ref type);
        }
    }
}