using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Yurowm;
using Yurowm.ContentManager;
using Yurowm.Effects;
using Yurowm.Extensions;
using Yurowm.Serialization;
using Yurowm.Utilities;
using LineInfo = Yurowm.Effects.LinedExplosionLogicProvider.LineInfo;

namespace YMatchThree.Core {
    public class LinedBombHit : BombHitBase {
        
        public const float hitSpeed = .05f; // Slots per 'this value' seconds;
        
        public ExplosionParameters explosion = new ExplosionParameters();
        public int waveOffset = 0;
        public Side side;
        
        Field field;
        Slots slots;
        
        List<Slot> hitGroup = new List<Slot>();
        List<LineInfo> lineInfos = new List<LineInfo>();

        HitContext hitContext;
        
        
        bool hitSelf;
        
        public void Prepare(bool hitSelf) {
            this.hitSelf = hitSelf;
            
            var center = epicenter.Center;
            
            hitGroup.AddRange(epicenter.Slots());

            slots = context.GetArgument<Slots>();
            
            field = slots.field;
            
            var lines = Sides.all
                .Where(s => side.HasFlag(s))
                .SelectMany(s => Enumerator.For(-waveOffset, waveOffset, 1)
                    .Select(i => new Line(s, i)))
                .ToList();
            
            for (int step = 1; lines.Count > 0; step++) {
                for (int i = 0; i < lines.Count; i++) {
                    var line = lines[i];
                    
                    var coord = center 
                                 + line.direction.ToInt2() * step
                                 + line.direction.Rotate(2).ToInt2() * line.offset;
                    
                    var slot = slots.all.Get(coord);
                    
                    if (slot) {
                        hitGroup.Add(slot);
                        
                        var lineInfo = lineInfos.FirstOrDefault(
                            i => i.Equal(line.direction, line.offset));
                        
                        if (lineInfo == null) {
                            lineInfo = new LinedExplosionLogicProvider.LineInfo(line.direction, line.offset);
                            lineInfos.Add(lineInfo);
                        }
                        
                        lineInfo.size = step.ClampMin(lineInfo.size);
                    }
                    
                    if (!field.Area.Contains(coord) || IsLineBreaker(slot)) {
                        lines.RemoveAt(i);
                        i--;
                    }
                }
            }
            
            hitGroup = hitGroup.NotNull().Distinct().ToList();
            
            hitContext = new HitContext(context, hitGroup, HitReason.BombExplosion);
            hitContext.driver = this;
        }
        
        public override IEnumerator Logic() {
            field.Highlight(hitContext.group, epicenter.Slot());

            if (!hitSelf)
                epicenter
                    .Slots()
                    .ForEach(s => hitGroup.Remove(s));

            var center = epicenter.Center;
            
            var time = field.time;
            
            for (int step = 0; hitGroup.Count > 0; step++) {
                hitGroup.RemoveAll(s => {
                    if (center.EightSideDistanceTo(s.coordinate) != step) return false;
                    
                    field.Explode(s.position, explosion);
                    s.HitAndScore(hitContext);
                    return true;
                });
                
                if (field.simulation.AllowToWait()) 
                    yield return time.Wait(hitSpeed);
            }

        }

        public override IEnumerable GetDestroyingEffectCallbacks() {
            yield return new LinedExplosionLogicProvider.Callback {
                lines = lineInfos
            };
        }

        public static bool IsLineBreaker(Slot slot) {
            if (slot && slot.GetCurrentContent() is ILineBreaker lineBreaker)
                return lineBreaker.BreakTheLine();
            return false;
        }

        public struct Line {
            public Side direction;
            public int offset;

            public Line(Side direction, int offset) {
                this.direction = direction;
                this.offset = offset;
            }
        }

        public override void Serialize(Writer writer) {
            base.Serialize(writer);
            
            writer.Write("side", side);
            writer.Write("offset", waveOffset);
            writer.Write("explosion", explosion);
        }

        public override void Deserialize(Reader reader) {
            base.Deserialize(reader);
            
            reader.Read("side", ref side);
            reader.Read("offset", ref waveOffset);
            reader.Read("explosion", ref explosion);
        }
    }
}