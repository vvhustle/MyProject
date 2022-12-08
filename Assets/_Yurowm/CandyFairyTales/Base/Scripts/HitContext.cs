using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Yurowm.ContentManager;

namespace YMatchThree.Core {
    public class HitContext {
        public Slot[] group = null;
        public HitReason reason = HitReason.Unknown;
        public object driver;

        public HitContext(LiveContext context, IEnumerable<Slot> group, HitReason reason = HitReason.Unknown) {
            this.group = group is Slot[] array ? array : group.ToArray();
            this.reason = reason;
            context.GetArgument<LevelEvents>().onHit(this);
        }
        
        public HitContext(LiveContext context, Slot slot, HitReason reason = HitReason.Unknown) : this(context, new[] { slot }, reason) { }
    }
    
    public enum HitReason {
        Unknown,
        Matching,
        BombExplosion, 
        Reaction,
        Player
    }
}