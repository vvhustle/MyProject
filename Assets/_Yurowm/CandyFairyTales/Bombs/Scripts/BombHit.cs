using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Yurowm.ContentManager;
using Yurowm.Dialogues;
using Yurowm.Extensions;
using Yurowm.Serialization;
using Yurowm.Utilities;

namespace YMatchThree.Core {
    public class BombHit : BombHitBase {
        
        public int distance = 1;
        public int2.DistanceType distanceType = int2.DistanceType.Eight;
        public ExplosionParameters explosion = new ExplosionParameters();
        
        List<Slot> hitGroup = new List<Slot>();
        HitContext hitContext;
        
        public int scorePoints {get; private set;} = 0;
        
        public void Prepare() {
            var slots = context.GetArgument<Slots>();
            
            var center = epicenter.Center;

            foreach (var slot in slots.all.Values) {
                if (slots.hidden.ContainsValue(slot)) continue;
                if (slot.coordinate.DistanceTo(center, distanceType) > distance) 
                    continue;
                
                var cc = slot.GetCurrentContent();
                if (cc is IDestroyable destroyable && !destroyable.destroying)
                    hitGroup.Add(slot);
            }

            hitGroup.AddRange(epicenter.Slots());
            
            hitContext = new HitContext(context, hitGroup.Distinct(), HitReason.BombExplosion);
            
            hitGroup.Reuse(hitContext.group);
        }

        public override IEnumerator Logic() {
            var slots = context.GetArgument<Slots>();
            var field = slots.field;
            
            field.Explode(epicenter.Position, explosion);
            
            field.Highlight(hitGroup.ToArray(), epicenter.Slot());
            
            scorePoints += hitGroup.Sum(s => s.Hit(hitContext));
            
            yield break;
        }

        public override void Serialize(Writer writer) {
            base.Serialize(writer);
            
            writer.Write("distance", distance);
            writer.Write("distanceType", distanceType);
            writer.Write("explosion", explosion);
        }

        public override void Deserialize(Reader reader) {
            base.Deserialize(reader);
            
            reader.Read("distance", ref distance);
            reader.Read("distanceType", ref distanceType);
            reader.Read("explosion", ref explosion);
        }
    }
}