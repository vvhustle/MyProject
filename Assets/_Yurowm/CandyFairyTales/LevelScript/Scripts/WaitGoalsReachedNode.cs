using System.Collections;
using Yurowm.Extensions;

namespace YMatchThree.Core {
    public class WaitGoalsReachedNode : ActionNode {
        
        public override IEnumerator Logic(object[] args) {
            var field = args?.CastOne<Field>();
            
            if (!field) yield break;
            
            var gameplay = field.fieldContext.Get<LevelGameplay>();

            while (!gameplay.IsLevelComplete())
                yield return null;

            Push(outputPort, field);
        }
    }
}