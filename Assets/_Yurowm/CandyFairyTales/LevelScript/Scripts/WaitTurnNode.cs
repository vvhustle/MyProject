using System.Collections;
using Yurowm.Coroutines;
using Yurowm.Extensions;
using Yurowm.Serialization;

namespace YMatchThree.Core {
    public class WaitTurnNode : ActionNode {
        
        public int turnsCount = 1;

        public override IEnumerator Logic(object[] args) {
            var field = args?.CastOne<Field>();
            
            if (!field) yield break;
            
            yield return Wait(turnsCount, field);
            
            Push(outputPort, field);
        }

        public static IEnumerator Wait(int turns, Field field) {
            if (!field || turns <= 0) yield break;
            
            LevelEvents events = field.fieldContext.GetArgument<LevelEvents>();

            void OnMove() => turns--;
            
            events.onMoveSuccess += OnMove;

            while (turns > 0)
                yield return null;
            
            events.onMoveSuccess -= OnMove;
        }
        
        #region ISerializable
        
        public override void Serialize(Writer writer) {
            base.Serialize(writer);
            writer.Write("count", turnsCount);
        }

        public override void Deserialize(Reader reader) {
            base.Deserialize(reader);
            reader.Read("count", ref turnsCount);
        }

        #endregion
    }
}