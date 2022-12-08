using System.Collections;
using System.Linq;
using Yurowm.Coroutines;
using Yurowm.Extensions;

namespace YMatchThree.Core {
    public class ClearFieldNode : ActionNode {
        
        public override IEnumerator Logic(object[] args) {
            var field = args?.CastOne<Field>();
            
            if (!field) yield break;
            
            var gameplay = field.fieldContext.Get<LevelGameplay>();

            using (var task = gameplay.NewExternalTask()) {
                yield return task.WaitAccess();
                
                field.fieldContext.GetAll<Chip>().ToArray().ForEach(c => c.HideAndKill());
                
                yield return new Wait(.3f);
                
                gameplay.NextTask<GravityTask>();
            }
            
            yield return gameplay.WaitForTask<GravityTask>();
            
            Push(outputPort, field);
        }
    }
}