using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Yurowm.Coroutines;
using Yurowm.Serialization;
using Yurowm.Utilities;

namespace YMatchThree.Core {
    public class BombsBooster : LevelBooster {
        
        public List<string> bombIDs;
        
        public ExplosionParameters spawnExplosion = new ExplosionParameters();
        
        protected override IEnumerator Logic() {
            Field field = null;

            yield return puzzleSpace.context.Catch<Field>(f => field = f);

            using (var task = field.gameplay.NewExternalTask()) {
                yield return task.WaitAccess();
                
                var targets = field.slots.all.Values
                    .Where(s => s.GetCurrentContent() is Chip c && c.IsDefault)
                    .ToList();

                foreach (var bombID in bombIDs) {
                    var target = targets.GrabRandom(random);
                    
                    if (!target) break;
                    
                    target.GetCurrentContent().HideAndKill();
                    
                    var bomb = storage.GetItemByID<BombChipBase>(bombID).Clone();
                    
                    bomb.emitType = SlotContent.EmitType.Script;
                    field.AddContent(bomb);
                    
                    field.Explode(target.position, spawnExplosion);
                            
                    target.AddContent(bomb);
                    bomb.localPosition = Vector2.zero;
                    
                    yield return new Wait(.2f);
                }
            }
            
            Redeem();
        }

        public override void Serialize(Writer writer) {
            base.Serialize(writer);
            writer.Write("bombs", bombIDs.ToArray());
            writer.Write("explosion", spawnExplosion);
        }

        public override void Deserialize(Reader reader) {
            base.Deserialize(reader);
            bombIDs = reader.ReadCollection<string>("bombs").ToList();
            reader.Read("explosion", ref spawnExplosion);
        }
    }
}