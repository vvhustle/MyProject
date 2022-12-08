using System.Collections.Generic;
using System.Linq;
using Yurowm.Serialization;
using Yurowm.Utilities;

namespace Yurowm.Sounds {
    public class SoundShuffle : SoundEffect {
        
        public List<string> paths = new List<string>();

        protected override string GetSoundPath() {
            return paths.GetRandom();
        }

        #region ISerializable

        public override void Serialize(Writer writer) {
            base.Serialize(writer);
            writer.Write("paths", paths.ToArray());
        }

        public override void Deserialize(Reader reader) {
            base.Deserialize(reader);
            paths = reader.ReadCollection<string>("paths").ToList();
        }

        #endregion
    }
}