using Yurowm.Dashboard;
using Yurowm.Serialization;
using Yurowm.Sounds;

namespace Yurowm.Editors {
    [DashboardGroup("Content")]
    [DashboardTab("Sounds", "Melody", "tab.sounds")]
    public class SoundStorageEditor : StorageEditor<SoundBase> {
        public override string GetItemName(SoundBase item) {
            return item.ID;
        }

        public override Storage<SoundBase> OpenStorage() {
            return SoundBase.storage;
        }
    }
}