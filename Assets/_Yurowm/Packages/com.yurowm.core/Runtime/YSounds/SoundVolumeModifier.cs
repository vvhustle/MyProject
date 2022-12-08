using UnityEngine;
using Yurowm.Serialization;
using Yurowm.Utilities;

namespace Yurowm.Sounds {
    public class SoundVolumeModifier : SoundModifier {
        public float min = .9f;
        public float max = .9f;
        
        public override void Apply(AudioSource source) {
            source.volume *= YRandom.main.Range(min, max);
        }

        public override void Serialize(Writer writer) {
            base.Serialize(writer);
            writer.Write("min", min);
            writer.Write("max", max);
        }

        public override void Deserialize(Reader reader) {
            base.Deserialize(reader);
            reader.Read("min", ref min);
            reader.Read("max", ref max);
        }
    }
}