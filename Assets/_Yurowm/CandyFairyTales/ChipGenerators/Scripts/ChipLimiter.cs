using System.Collections;
using System.Linq;
using Yurowm.Extensions;
using Yurowm.Serialization;
using Yurowm.Spaces;

namespace YMatchThree.Core {
    public class ChipLimiter : LevelExtension {
        public string chipID;
        public int count;

        int currentCount = 0;
        LevelEvents events;
        Slots slots;
        
        ChipGeneratorController controller;
        
        public override void OnAddToSpace(Space space) {
            base.OnAddToSpace(space);
            events = context.GetArgument<LevelEvents>();
            slots = context.Get<Slots>();
            events.onStartTask += OnStartTask;
            events.onGenerate += OnGenerate;
            
            controller = context.Get<ChipGeneratorController>();
            controller.limiters.Add(this);
        }

        public override void OnRemoveFromSpace(Space space) {
            base.OnRemoveFromSpace(space);
            events.onStartTask -= OnStartTask;
            events.onGenerate -= OnGenerate;
            controller.limiters.Remove(this);
        }

        void OnStartTask(LevelGameplay.Task task) {
            if (task is GravityTask) {
                currentCount = slots.all.Values
                    .Where(s => s.visible)
                    .Select(s => s.GetContent<Chip>())
                    .NotNull()
                    .Count(c => c.ID == chipID);
            }
        }

        void OnGenerate(Chip chip) {
            if (chip.ID == chipID)
                currentCount++;
        }

        public bool IsAllowed(LevelContent chip) {
            return chip.ID != chipID || currentCount < count;
        }

        public override IEnumerator GetVariblesTypes() {
            yield return base.GetVariblesTypes();
            yield return typeof(ContentIDVariable);
            yield return typeof(CountVariable);
        }

        public override void SetupVariable(ISerializable variable) {
            base.SetupVariable(variable);

            switch (variable) {
                case ContentIDVariable cid:
                    chipID = cid.id;
                    break;
                case CountVariable c:
                    count = c.value;
                    break;
            }
        }
    }
}