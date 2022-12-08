using System.Collections;
using YMatchThree.Meta;
using Yurowm.Core;
using Yurowm.Extensions;
using Yurowm.Localizations;
using Yurowm.Serialization;
using Yurowm.UI;

namespace Yurowm.Store {
    public abstract class InventoryItem : StoreItem, ILocalized {
        public IEnumerable GetLocalizationKeys() {
            yield break;
        }
    }
    
    public class Currency : InventoryItem {
        public string symbol;
        
        [ReferenceValueLoader]
        static void AddReferences() {
            storage
                .Items<Currency>()
                .ForEach(c => {
                    ReferenceValues.Add(c.ID, () => PlayerData.inventory.GetItemCount(c.ID));
                    ReferenceValues.Add(c.ID + "_symbol", () => c.symbol);
                });
        }

        public override void Serialize(Writer writer) {
            base.Serialize(writer);
            writer.Write("symbol", symbol);
        }

        public override void Deserialize(Reader reader) {
            base.Deserialize(reader);
            reader.Read("symbol", ref symbol);
        }
    }
    
}