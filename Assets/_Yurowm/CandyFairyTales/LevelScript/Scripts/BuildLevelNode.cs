using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Yurowm.ComposedPages;
using Yurowm.Coroutines;
using Yurowm.DebugTools;
using Yurowm.Extensions;
using Yurowm.Nodes;
using Yurowm.Serialization;
using Yurowm.UI;
using Page = Yurowm.UI.Page;
using Space = Yurowm.Spaces.Space;

namespace YMatchThree.Core {
    public class BuildLevelNode : ActionNode {
        
        public Level level;
        
        Field field;
        
        public override string IconName => "LevelIcon";

        public override IEnumerator Logic(object[] args) {
            if (field != null && field.IsAlive()) yield break;
            
            var incomeField = args.CastOne<Field>();
            
            if (incomeField != null) {
                yield return incomeField.gameplay?.WaitForTask<WaitTask>();
                yield return incomeField.Hiding();
                incomeField.Kill();
            }
            
            var space = context.GetArgument<PuzzleSpace>();

            field = new Field(level.Clone(), space);
            space.AddItem(field);
            
            yield return field.BuildStage();
            
            space.scriptEvents.onPlayerAttention.Invoke();

            while (Page.GetCurrent()?.ID != "Field")
                yield return null;

            yield return field.Showing();
            
            field.events.onLevelFailed = OnLevelFail;

            Push(outputPort, field);
            
            DebugPanel.Log("Hit All Chips", "Gameplay", () => {
                var hitContext = new HitContext(field.fieldContext, 
                    field.fieldContext.GetAll<Chip>().Select(c => c.slotModule.Slot()));
                hitContext.group.ForEach(c => c.Hit(hitContext));
            });
            
            field.Start();
            
            UIRefresh.Invoke();
        }

        void OnLevelFail() {
            if (field) {
                var space = context.GetArgument<Space>();
                field.Hiding()
                    .ContinueWith(field.Kill)
                    .Run(space.coroutine);
            }
            Page.Get("LevelFailed").Show();
        }

        public override IEnumerable<object> OnPortPulled(Port sourcePort, Port targetPort) {
            if (targetPort == outputPort)
                yield return field;
        }

        public override void Serialize(Writer writer) {
            base.Serialize(writer);
            writer.Write("level", level);
        }

        public override void Deserialize(Reader reader) {
            base.Deserialize(reader);
            reader.Read("level", ref level);
        }
    }
}