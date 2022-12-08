using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Yurowm.Extensions;
using Yurowm.Serialization;
using Yurowm.Utilities;
using Space = Yurowm.Spaces.Space;

namespace YMatchThree.Core {
    public class Virus : BlockChip {
        VirusReaction reaction;
        
        public ExplosionParameters eating = new ExplosionParameters();

        public override void OnAddToSpace(Space space) {
            base.OnAddToSpace(space);
            reaction = Reactions.Get(field).Emit<VirusReaction>();
        }
        
        public override IEnumerator Destroying() {
            reaction.hit = true;
            return base.Destroying();
        }

        public override void Serialize(Writer writer) {
            base.Serialize(writer);
            writer.Write("eating", eating);
        }

        public override void Deserialize(Reader reader) {
            base.Deserialize(reader);
            reader.Read("eating", ref eating);
        }
    }
    
    public class VirusReaction : Reaction {
        
        public bool hit;
        
        LevelEvents events;

        public override int GetPriority() {
            return 1;
        }

        public override Type GetReactionType() {
            return Type.Move;
        }

        public override void Initialize() {
            base.Initialize();
            events = context.GetArgument<LevelEvents>();
            events.onStartTask += OnStartTask;
        }

        void OnStartTask(LevelGameplay.Task task) {
            if (task is WaitTask)
                hit = false;
        }

        public override void OnKill() {
            base.OnKill();
            events.onStartTask -= OnStartTask;
        }

        public override IEnumerator React() {
            if (!context.Contains<Virus>()) {
                Kill();
                yield break;
            }
            
            var virusOriginal = storage.items.CastOne<Virus>();
            
            if (!virusOriginal) {
                Kill();
                yield break;
            }
            
            if (hit) yield break;

            var infectedSlots = context
                .GetAll<Virus>()
                .Where(i => i.slotModule.HasSlot())
                .SelectMany(i => i.slotModule.Slots())
                .ToArray();
            
            if (infectedSlots.Length == 0) yield break;
            
            var all = infectedSlots.ToList();
            
            var newInfectedSlots = new List<Slot>();
            
            var chunk = new List<Slot>();
            
            while (all.Count > 0) {
                chunk.Clear();
                chunk.Add(all.FirstOrDefault());

                while (true) {
                    var near = all
                        .Exclude(chunk)
                        .FirstOrDefault(s => 
                            chunk.Any(c => c.coordinate.FourSideDistanceTo(s.coordinate) == 1));
                    if (!near) break;
                    chunk.Add(near);
                }
                
                all.RemoveRange(chunk);
                
                var target = chunk
                    .SelectMany(x => x.nearSlot.Where(y => y.Key.IsStraight())
                        .Select(z => z.Value))
                    .Distinct()
                    .NotNull()
                    .Exclude(infectedSlots)
                    .Exclude(newInfectedSlots)
                    .Where(s => s.GetCurrentContent() is Chip)
                    .GetRandom(random);
                
                if (target) {
                    newInfectedSlots.Add(target);
                    target.GetCurrentContent().HideAndKill();
                    
                    var newVirus = virusOriginal.Clone();
                    
                    field.AddContent(newVirus);
                    newVirus.position = target.position;
                    
                    target.AddContent(newVirus);
                    
                    field.Explode(target.position, newVirus.eating);
                }
            }

            yield return Reactor.Result.CompleteToGravity;
        }
    }
}