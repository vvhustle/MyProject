using System.Collections;
using Yurowm.Nodes;
using Yurowm.Utilities;

namespace Yurowm.Core {
    public class AdShowNode : UserPathFilter {
        
        public readonly Port successPort = new Port(2, "Success", Port.Info.Output, Side.Bottom);
        public readonly Port failedPort = new Port(3, "Failed", Port.Info.Output, Side.Bottom);

        public override IEnumerable GetPorts() {
            yield return base.GetPorts();
            yield return successPort;
            yield return failedPort;
        }

        public override IEnumerator Logic() {
            if (Adverts.IsReady(AdType.Interstitial)) {
                var wait = true;
                
                Adverts.ShowAds(callback: c => wait = c == Adverts.Callback.Start);

                while (wait)
                    yield return null;
                
                Push(successPort);
            } else
                Push(failedPort);
        }
    }
}