using System.Collections;
using Yurowm.Extensions;

namespace YMatchThree.Core {
    public class WaitTaskNode : ActionNode {
        
        public override IEnumerator Logic(object[] args) {
            var field = args?.CastOne<Field>();
            
            if (!field) yield break;
            
            yield return Wait(field);
            
            Push(outputPort, field);
        }
        
        public static IEnumerator Wait(Field field) {
            yield return field.fieldContext
                .Get<LevelGameplay>()
                .WaitForTask<WaitTask>();
        }
    }
}