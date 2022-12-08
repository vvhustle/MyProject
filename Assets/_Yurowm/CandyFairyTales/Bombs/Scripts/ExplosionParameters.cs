using Yurowm.Serialization;

namespace YMatchThree.Core {
    public class ExplosionParameters : ISerializable {
        public float radius;
        public float force;

        public void Serialize(Writer writer) {
            writer.Write("radius", radius);
            writer.Write("force", force);
        }

        public void Deserialize(Reader reader) {
            reader.Read("radius", ref radius);
            reader.Read("force", ref force);
        }
    }
}