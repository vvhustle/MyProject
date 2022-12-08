using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Yurowm.Extensions;
using Yurowm.Nodes;
using Yurowm.Utilities;

namespace YMatchThree.Core {
    public abstract class FieldEventNode : LevelScriptNode {
        
        protected Port outputPort = new Port(0, "Output", Port.Info.Output, Side.Bottom);
        protected Port eventsPort = new Port(1, "Events", Port.Info.Input, Side.Top);

        List<Field> registeredField = new List<Field>();

        public abstract void RegisterEvent(Field field);
        
        public override IEnumerable GetPorts() {
            yield return outputPort;
            yield return eventsPort;
        }

        public override void OnPortPushed(Port sourcePort, Port targetPort, object[] args) {
            var field = args?.CastOne<Field>();
            if (!field) return;
            
            if (targetPort == eventsPort && !registeredField.Contains(field)) {
                registeredField.Add(field);
                RegisterEvent(field);
            }
        }
    }
}