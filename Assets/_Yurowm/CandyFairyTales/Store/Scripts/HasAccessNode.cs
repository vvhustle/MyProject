using YMatchThree.Meta;
using Yurowm.Core;
using Yurowm.Extensions;
using Yurowm.Serialization;

namespace Yurowm.Store {
    public class HasAccessNode : UserPathValueProvider {
        
        public string accessKey;
        
        protected override bool TryGetValue(out object value) {
            if (accessKey.IsNullOrEmpty()) {
                value = default;
                return false;
            }
            
            value = PlayerData.store.HasAccess(accessKey);

            return true;
        }

        public override void Serialize(Writer writer) {
            base.Serialize(writer);
            writer.Write("accessKey", accessKey);
        }

        public override void Deserialize(Reader reader) {
            base.Deserialize(reader);
            reader.Read("accessKey", ref accessKey);
        }
    }
}