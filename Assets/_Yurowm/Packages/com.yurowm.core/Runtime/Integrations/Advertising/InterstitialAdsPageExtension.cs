using Yurowm.UI;

namespace Yurowm.Advertising {
    public class InterstitialAdsPageExtension : PageExtension {
        
        public override void OnShow() {
            base.OnShow();
            Adverts.ShowAds();
        }
        
    }
}