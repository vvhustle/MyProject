using System.Linq;
using Yurowm.Extensions;
using Yurowm.Serialization;

namespace YMatchThree.Core {
    public class LevelScriptOrderedPreviews : ISerializable {
        public LevelScriptOrdered[] scripts;
        
        public void Serialize(Writer writer) {
            scripts.ForEach(l => l.preloadMode = true);
            writer.Write("scripts", scripts);
            scripts.ForEach(l => l.preloadMode = false);
        }

        public void Deserialize(Reader reader) {
            scripts = reader.ReadCollection<LevelScriptOrdered>("scripts")
                .ToArray();
            scripts.ForEach(l => l.MarkAsPreview());
        }
    }
}