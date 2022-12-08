using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Yurowm.Coroutines;
using Yurowm.Extensions;
using Yurowm.Nodes;
using Yurowm.Serialization;
using Yurowm.Utilities;
using Space = Yurowm.Spaces.Space;

namespace YMatchThree.Core {
    public class EmitBombsNode : ActionNode {
        
        public int count = 1;
        public string slotTag = "";
        
        public List<string> bombIDs = new List<string>();
        
        public override IEnumerator Logic(object[] args) {
            var field = args?.CastOne<Field>();
            
            if (!field || field.complete)
                yield break;
            
            var random = field.random.NewRandom($"Node{ID}");
            
            var bombs = LevelContent.storage
                .Items<BombChipBase>()
                .Where(b => bombIDs.Contains(b.ID))
                .ToArray();
            
            if (bombs.Length > 0 ) {
                var gameplay = field.fieldContext.Get<LevelGameplay>();

                using (var task = gameplay.NewExternalTask()) {
                    
                    yield return task.WaitAccess();
                    
                    var targets = field.slots.all.Values
                        .Where(s => s.GetCurrentContent() is Chip c && c.IsDefault)
                        .Where(s => slotTag.IsNullOrEmpty() || (s.GetContent<SlotTag>()?.HasTag(slotTag) ?? false))
                        .ToList();
                    
                    for (int i = 0; i < count; i++) {
                        var target = targets.GrabRandom();
                        if (!target) break;
                        
                        var chip = target.GetCurrentContent();
                        
                        var color = chip is IColored colored ? colored.colorInfo : ItemColorInfo.None;
                        
                        chip.HideAndKill();
                        
                        var bombInfo = bombs.GetRandom(random);
                        
                        chip = bombInfo.Clone();
                        
                        if (color.type == ItemColor.Known) {
                            chip.SetupVariable(new ColoredVariable {
                                info = color
                            });
                        }
                    
                        chip.emitType = SlotContent.EmitType.Script;
                        field.AddContent(chip);
                        
                        target.AddContent(chip);
                        chip.localPosition = Vector2.zero;
                    
                        yield return gameplay.time.Wait(0.1f);
                    }
                    
                    gameplay.matchDate++;
                    
                    gameplay.NextTask<MatchingTask>();
                }
                
                
            } else
                yield return null;
            
            Push(outputPort, field);
        }

        #region ISerializable
        
        public override void Serialize(Writer writer) {
            base.Serialize(writer);
            writer.Write("count", count);
            writer.Write("tag", slotTag);
            writer.Write("bombs", bombIDs.ToArray());
        }

        public override void Deserialize(Reader reader) {
            base.Deserialize(reader);
            reader.Read("count", ref count);
            reader.Read("tag", ref slotTag);
            bombIDs.Clear();
            bombIDs.AddRange(reader.ReadCollection<string>("bombs"));
        }

        #endregion
    }
}