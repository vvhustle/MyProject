using System.Collections;
using Yurowm.Coroutines;
using Yurowm.Nodes;
using Yurowm.Spaces;
using Yurowm.Utilities;

namespace YMatchThree.Core {
    public abstract class ActionNode : LevelScriptNode {
        
        public readonly Port triggerPort = new Port(0, "Trigger", Port.Info.Input, Side.Top);
        public readonly Port outputPort = new Port(1, "Output", Port.Info.Output, Side.Bottom);
        
        public override IEnumerable GetPorts() {
            yield return triggerPort;
            yield return outputPort;
        }
        
        public override void OnPortPushed(Port sourcePort, Port targetPort, object[] args) {
            if (targetPort == triggerPort)
                OnTrigger(args);   
        }
        
        public abstract IEnumerator Logic(object[] args);

        public virtual void OnTrigger(object[] args) {
            var coroutineCore = context.GetArgument<Space>()?.coroutine;
            Logic(args).Run(coroutineCore);
        }
    }
}