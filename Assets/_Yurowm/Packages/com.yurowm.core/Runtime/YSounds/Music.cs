using Yurowm.Coroutines;
using Yurowm.Serialization;

namespace Yurowm.Sounds {
    public class Music : SoundBase {
        
        public string path;
        
        public override void Play() {
            SoundController.GetClip(path, SoundController.PlayMusic).Run();
        }

        public override void Serialize(Writer writer) {
            base.Serialize(writer);
            writer.Write("path", path);
        }

        public override void Deserialize(Reader reader) {
            base.Deserialize(reader);
            reader.Read("path", ref path);
        }
    }
}