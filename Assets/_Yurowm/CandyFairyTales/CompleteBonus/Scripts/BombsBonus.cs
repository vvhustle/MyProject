using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Yurowm;
using Yurowm.Coroutines;
using Yurowm.Extensions;
using Yurowm.Serialization;
using Yurowm.Utilities;
using Space = Yurowm.Spaces.Space;

namespace YMatchThree.Core {
    public class BombsBonus : CompleteBonus {
        
        public int count = 8;
        
        public List<string> bombIDs;
        public Chip[] bombs;

        LevelGameplay gameplay;
        Slots slots;
        
        static readonly SlotContent.EmitType hitBombMask = ~SlotContent.EmitType.Generated;
        
        public ExplosionParameters explosion = new ExplosionParameters();
        
        public override void OnAddToSpace(Space space) {
            base.OnAddToSpace(space);
            context.SetupItem(out gameplay);
            slots = context.GetArgument<Slots>();
            bombs = storage.Items<Chip>().Where(b => b is BombChipBase && bombIDs.Contains(b.ID))
                .ToArray();
        }

        public override bool IsComplete() {
            return bombIDs.Count == 0 ||
                   !(gameplay.AllowToMove() || context.Contains<BombChipBase>(
                       b => b.visible && b.emitType.OverlapFlag(hitBombMask)));
        }

        public override IEnumerator Logic() {
            int units = GetBonusUnits(count);

            if (units > 0) {

                var targets = context
                    .GetArgument<Slots>().all.Values
                    .Where(s => s.GetCurrentContent() is Chip c && c.IsDefault)
                    .ToList();

                for (int i = 0; i < units; i++) {
                    gameplay.OnNextTurn();

                    var target = targets.GrabRandom(random);
                    if (!target) break;

                    var chip = target.GetCurrentContent();

                    var color = chip is IColored colored ? colored.colorInfo : ItemColorInfo.None;

                    chip.Kill();

                    var nextBomb = bombs.GetRandom(random);

                    chip = nextBomb.Clone();

                    if (color.type == ItemColor.Known) {
                        chip.SetupVariable(new ColoredVariable {
                            info = color
                        });
                    }

                    chip.emitType = SlotContent.EmitType.Script;
                    field.AddContent(chip);

                    target.AddContent(chip);
                    chip.localPosition = Vector2.zero;

                    field.Explode(target.position, explosion);

                    if (simulation.AllowToWait())
                        yield return time.Wait(0.1f);
                }

                gameplay.matchDate++;

                if (simulation.AllowToWait())
                    yield return time.Wait(0.1f);
            } 

            HitBombs().Run(field.coroutine);
        }
        
        IEnumerator HitBombs() {
            gameplay.NextTask<MatchingTask>();

            var pool = MatchingPool.Get(field.fieldContext);
            
            yield return pool.WaitOpen();
                
            using (pool.Use()) {
                while (true) {
                    var bomb = slots.all.Values
                        .Select(s => s.GetCurrentContent())
                        .NotNull()
                        .Where(s => s.visible)
                        .CastIfPossible<BombChipBase>()
                        .Where(b => !b.destroying && b.emitType.OverlapFlag(hitBombMask))
                        .GetRandom(random);

                    if (bomb == null)
                        yield break;


                    bomb.HitAndScore(new HitContext(context, bomb.slotModule.Slot(), HitReason.Reaction));
                    gameplay.matchDate++;
                    
                    if (simulation.AllowToWait()) 
                        yield return time.Wait(0.05f);
                    else 
                        yield return null;
                }
            }
        }

        public override void Serialize(Writer writer) {
            base.Serialize(writer);
            writer.Write("count", count);
            writer.Write("bombs", bombIDs.ToArray());
            writer.Write("explosion", explosion);
        }

        public override void Deserialize(Reader reader) {
            base.Deserialize(reader);
            reader.Read("count", ref count);
            bombIDs = reader.ReadCollection<string>("bombs").ToList();
            reader.Read("explosion", ref explosion);
        }
    }
}