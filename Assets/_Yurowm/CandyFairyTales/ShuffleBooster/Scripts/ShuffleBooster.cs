using System.Collections;
using System.Linq;
using UnityEngine;
using Yurowm;
using Yurowm.Coroutines;
using Yurowm.Extensions;
using Yurowm.Serialization;
using Yurowm.Spaces;
using Yurowm.Utilities;

namespace YMatchThree.Core {
    public class ShuffleBooster : FieldBoosterSlotBased {
        public ExplosionParameters putChipBouncing = new ExplosionParameters();
        
        protected override bool IsAutoredeem => false;
        
        protected override bool FilterSlot(Slot slot) {
            return slot.GetCurrentContent();
        }

        protected override IEnumerator SlotLogic(Slot slot) {
            position = field.slots.edge.center;
            
            var open = false;

            PlayAnimation(Callback).Run(field.coroutine);
            
            void Callback(string callbackName) {
                if (callbackName == "Open")
                    open = true;
                if (callbackName == "Shake")
                    sound?.Play(callbackName);
                if (callbackName == "Fill")
                    sound?.Play(callbackName);
            }
            
            var chips = context.GetArgument<Slots>().all.Values
                .Select(s => s.GetCurrentContent())
                .CastIfPossible<Chip>()
                .ToArray();

            while (!open) yield return null;

            yield return chips.Select(c => MoveChip(c, position)).Parallel();
            
            chips.ForEach(c => c.enabled = false);
            
            if (gameplay.Shuffle(chips, false))
                Redeem();
            
            open = false;
            
            while (!open) yield return null;
            
            chips.ForEach(c => c.enabled = true);
            
            yield return chips.Select(c => {
                var point = c.slotModule.Position;
                return MoveChip(c, point)
                    .ContinueWith(() => field.Explode(point, putChipBouncing));
            }).Parallel();
            
            gameplay.NextTask<GravityTask>();
        }

        IEnumerator MoveChip(Chip c, Vector2 targetPosition) {
            var startPosition = c.position;
            
            yield return new Wait(random.Range(0, .3f));
            
            var duraion = random.ValueRange(1f, 1.5f);
            
            for (var t = 0f; t < 1f; t += time.Delta / duraion) {
                
                c.position = Vector2.Lerp(startPosition, targetPosition, t.Ease(EasingFunctions.Easing.InOutCubic))
                    + Vector2.up * (YMath.SinNatural(t) * Slot.Offset * 2);
                
                yield return null;
            }

            c.position = targetPosition;
        }

        public override void Serialize(Writer writer) {
            base.Serialize(writer);
            writer.Write("putChipBouncing", putChipBouncing);
        }

        public override void Deserialize(Reader reader) {
            base.Deserialize(reader);
            reader.Read("putChipBouncing", ref putChipBouncing);
        }
    }
}