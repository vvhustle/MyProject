using Yurowm.Integrations;
using Yurowm.Serialization;

namespace Yurowm.Analytics {
    public abstract class AnalyticIntegration : Integration {
        
        public bool trackAll = true;
        
        public override void Initialize() {
            base.Initialize();
            Analytic.integrations.Add(this);
        }

        public abstract void Event(string eventName);

        public virtual void Event(string eventName, params Segment[] segments) {
            Event(eventName);
        }

        #region ISerializable

        public override void Serialize(Writer writer) {
            writer.Write("trackAll", trackAll);
            base.Serialize(writer);
        }

        public override void Deserialize(Reader reader) {
            reader.Read("trackAll", ref trackAll);
            base.Deserialize(reader);
        }

        #endregion
    }
}