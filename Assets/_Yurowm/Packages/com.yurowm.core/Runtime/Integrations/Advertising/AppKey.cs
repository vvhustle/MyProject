using Yurowm.Serialization;

namespace Yurowm.Advertising {
    public class AppKey : ISerializable {
        public string key;
        public AdPlatform platform;
        
        public void Serialize(Writer writer) {
            writer.Write("key", key);
            writer.Write("platform", platform);
        }

        public void Deserialize(Reader reader) {
            reader.Read("key", ref key);
            reader.Read("platform", ref platform);
        }
    }
}