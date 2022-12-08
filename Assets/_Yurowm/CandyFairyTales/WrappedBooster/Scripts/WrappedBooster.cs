using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Yurowm;
using Yurowm.Coroutines;
using Yurowm.Effects;
using Yurowm.Extensions;
using Yurowm.Serialization;
using Yurowm.Utilities;
using LineInfo = Yurowm.Effects.LinedExplosionLogicProvider.LineInfo;

namespace YMatchThree.Core {
    public class WrappedBooster : FieldBoosterSlotBased {
        
        public BombHit hit = new BombHit();
        
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
                    Explosion(slot).Run(field.coroutine);
                }
                
                PlayAnimation(Hit).Run(field.coroutine);

                while (!hit)
                    yield return null;
            }
        }

        IEnumerator Explosion(Slot hitSlot) {
            hit.Initialize(hitSlot.GetTempModule(), context);

            hit.Prepare();
            
            yield return hit.Logic();
        }

        public override void Serialize(Writer writer) {
            base.Serialize(writer);
            writer.Write("hit", hit);
        }

        public override void Deserialize(Reader reader) {
            base.Deserialize(reader);
            reader.Read("hit", ref hit);
        }
    }
}