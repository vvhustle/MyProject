using System.Collections;
using Yurowm.Nodes;
using Yurowm.Utilities;

namespace YMatchThree.Core {
    public class OnLaunchNode : LevelScriptNode {
        
        public readonly Port outputPort = new Port(0, "Output", Port.Info.Output, Side.Bottom);
        
        public override string IconName => "Exception";
        
        public override IEnumerable GetPorts() {
            yield return outputPort;
        }

        public override void OnPortPushed(Port sourcePort, Port targetPort, object[] args) {}

        public override void OnLauch() {
            base.OnLauch();
            Push(outputPort);
        }
    }
}