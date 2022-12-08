using Yurowm.Serialization;
using Yurowm.Sounds;

namespace Yurowm.Audio {
    public class AudioSettings : SettingsModule {
                
        float soundsVolume = 1;
        float musicVolume = 1;
        public bool Mute => soundsVolume <= 0;

        public override void Initialize() {
            SoundVolume = soundsVolume;
            MusicVolume = musicVolume;
        }
        
        public float SoundVolume {
            get => soundsVolume;
            set {
                var v = value.Clamp01();
                if (soundsVolume != v) {
                    soundsVolume = v;
                    SetDirty();
                }
                SoundController.SoundVolume = v;
            }
        }
        
        public float MusicVolume {
            get => musicVolume;
            set {
                var v = value.Clamp01();
                if (musicVolume != v) {
                    musicVolume = v;
                    SetDirty();
                }
                SoundController.MusicVolume = v;
            }
        }
        
        public override void Serialize(Writer writer) {
            writer.Write("soundsVolume", soundsVolume);
            writer.Write("musicVolume", musicVolume);
        }

        public override void Deserialize(Reader reader) {
            reader.Read("soundsVolume", ref soundsVolume);
            reader.Read("musicVolume", ref musicVolume);
        }
    }
}