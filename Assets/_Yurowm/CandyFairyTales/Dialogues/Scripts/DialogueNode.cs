using System.Collections;
using System.Collections.Generic;
using System.Linq;
using YMatchThree;
using YMatchThree.Core;
using Yurowm.Coroutines;
using Yurowm.Extensions;
using Yurowm.Localizations;
using Yurowm.Serialization;

namespace Yurowm.Dialogues {
    public class DialogueNode : ActionNode, ILocalizedLevelScriptNode {

        public List<Instruction> instructions = new List<Instruction>();
        
        public override string IconName => "Speech";

        public override IEnumerator Logic(object[] args) {
            var field = args?.CastOne<Field>();
            var space = context.GetArgument<PuzzleSpace>();

            if (!instructions.IsEmpty()) {
                if (!context.SetupItem(out Dialogue dialogue)) {
                    dialogue = new Dialogue();
                    space.AddItem(dialogue);
                }
                
                space.scriptEvents.onPlayerAttention.Invoke();
                
                if (field) 
                    yield return WaitTaskNode.Wait(field);
                
                while (dialogue.IsActing)
                    yield return null;
                
                using (dialogue.Act())
                    yield return instructions.Select(i => i.Logic(dialogue)).Parallel();

                dialogue.Release();
            }
            
            Push(outputPort, field);
        }

        public IEnumerable<string> GetLocalizationKeys(LevelScriptBase level) {
            foreach (var localized in instructions)
                foreach (var key in localized.GetLocalizationKeys()) 
                    yield return level.GetFullLocalizationKey(key);

        }

        #region ISerializable

        public override void Serialize(Writer writer) {
            base.Serialize(writer);
            writer.Write("instructions", instructions.ToArray());
        }

        public override void Deserialize(Reader reader) {
            base.Deserialize(reader);
            instructions.Reuse(reader.ReadCollection<Instruction>("instructions"));
        }

        #endregion
    }
}