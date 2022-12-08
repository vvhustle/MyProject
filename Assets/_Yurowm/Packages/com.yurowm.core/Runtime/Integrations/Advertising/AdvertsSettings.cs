using Yurowm.Serialization;

namespace Yurowm.Advertising {
    public class AdvertsSettings : SettingsModule {
        
        public bool adsEnabled = true;
        public bool rewardedAdsEnabled = true;
        
        public AdvertsSettings() : base() { }

        public void SetADsEnabled(bool value) {
            if (adsEnabled == value) return;
            adsEnabled = value;
            SetDirty();
        }  

        public void SetRewardedAdsEnable(bool value) {
            if (rewardedAdsEnabled == value) return;
            rewardedAdsEnabled = value;
            SetDirty();
        }
        
        public override void Serialize(Writer writer) {
            writer.Write("ad", adsEnabled);
            writer.Write("rewardedAds", rewardedAdsEnabled);
            
        }

        public override void Deserialize(Reader reader) {
            reader.Read("ad", ref adsEnabled);
            reader.Read("rewardedAds", ref rewardedAdsEnabled);
        }
    }
}