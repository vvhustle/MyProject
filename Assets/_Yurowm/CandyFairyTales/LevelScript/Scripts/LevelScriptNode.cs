using System.Collections.Generic;
using Yurowm.ContentManager;
using Yurowm.Localizations;
using Yurowm.Nodes;

namespace YMatchThree.Core {
    public abstract class LevelScriptNode : Node {
        public LiveContext context;
        public virtual void OnLauch() {}
    }

    public interface ILocalizedLevelScriptNode {
        
        IEnumerable<string> GetLocalizationKeys(LevelScriptBase level);
    }
}