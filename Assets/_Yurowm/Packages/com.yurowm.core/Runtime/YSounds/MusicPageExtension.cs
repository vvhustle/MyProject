using System.Linq;
using Yurowm.Serialization;
using Yurowm.UI;

namespace Yurowm.Sounds {
    public class MusicPageExtension : PageExtension {
        public string clip;
        public float volume = 1f;

        public override void OnShow() {
            base.OnShow();
            
            SoundController.MusicVolumeMultiplier = volume;
            
            SoundBase.storage
                .Items<Music>()
                .FirstOrDefault(s => s.ID == clip)?
                .Play();
        }

        public override void Serialize(Writer writer) {
            base.Serialize(writer);
            writer.Write("clip", clip);
            writer.Write("volume", volume);
        }

        public override void Deserialize(Reader reader) {
            base.Deserialize(reader);
            reader.Read("clip", ref clip);
            reader.Read("volume", ref volume);
        }
    }
}