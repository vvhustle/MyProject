using Yurowm.Serialization;

namespace Yurowm {
    public class VibratorSettings : SettingsModule {
        public bool vibeEnabled = true;

        public override void Initialize() {
            base.Initialize();
            SetVibeEnable(vibeEnabled);
        }
        
        public void SetVibeEnable(bool value) {
            Vibrator.Active = value;
            if (vibeEnabled != value) {
                vibeEnabled = value;
                SetDirty();
                Vibrator.AndroidVibrate(.3f);
                Vibrator.iOSVibrate();
            }
        }

        public override void Serialize(Writer writer) {
            writer.Write("vibe", vibeEnabled);
            
        }

        public override void Deserialize(Reader reader) {
            reader.Read("vibe", ref vibeEnabled);
        }
    }
}