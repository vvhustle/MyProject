using System.Collections;
using Yurowm.Utilities;

namespace Yurowm.Core {
    public class AdPreloadNode : UserPathFilter {
        public override IEnumerator Logic() {
            var access = new DelayedAccess(1);

            while (true) {
                if (access.GetAccess()) {
                    if (Adverts.IsReady(AdType.Interstitial))
                        yield break;
                    
                    Adverts.Preload(AdType.Interstitial);
                }
                
                yield return null;
            }
        }
    }
}