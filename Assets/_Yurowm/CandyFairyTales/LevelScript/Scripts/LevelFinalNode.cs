using System.Collections;
using System.Collections.Generic;
using Yurowm.Coroutines;
using Yurowm.Nodes;
using Yurowm.Utilities;
using Yurowm.Spaces;

namespace YMatchThree.Core {
    public abstract class LevelFinalNode : LevelScriptNode {
        public readonly Port triggerPort = new Port(0, "Trigger", Port.Info.Input, Side.Top);
        
        public override IEnumerable GetPorts() {
            yield return triggerPort;
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