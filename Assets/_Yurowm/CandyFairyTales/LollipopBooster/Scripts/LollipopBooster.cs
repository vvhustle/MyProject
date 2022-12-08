using System.Collections;
using Yurowm.Coroutines;
using Yurowm.Serialization;

namespace YMatchThree.Core {
    public class LollipopBooster : FieldBoosterSlotBased {
        
        public ExplosionParameters hitBouncing = new ExplosionParameters();
    
        protected override bool IsAutoredeem => true;
        
        protected override bool FilterSlot(Slot slot) {
            var currentContent = slot.GetCurrentContent();
            return currentContent && currentContent is IDestroyable;
        }

        protected override IEnumerator SlotLogic(Slot slot) {
            gameplay.NextTask<MatchingTask>();
                
            var pool = MatchingPool.Get(field.fieldContext);
            
            yield return pool.WaitOpen();
                
            using (pool.Use()) {
                position = slot.position;
                
                var hit = false;
                
                void Hit(string _) {
                    hit = true;
                    field.Explode(slot.position, hitBouncing);
                    slot.HitAndScore(new HitContext(context, new[] {slot}, HitReason.Player));
                }
                
                PlayAnimation(Hit).Run(field.coroutine);

                while (!hit)
                    yield return null;
            }
        }

        public override void Serialize(Writer writer) {
            base.Serialize(writer);
            writer.Write("hitBouncing", hitBouncing);
        }

        public override void Deserialize(Reader reader) {
            base.Deserialize(reader);
            reader.Read("hitBouncing", ref hitBouncing);
        }
    }
}