using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Yurowm;
using Yurowm.ContentManager;
using Yurowm.Serialization;
using Yurowm.Spaces;
using Yurowm.Utilities;
using Space = Yurowm.Spaces.Space;

namespace YMatchThree.Core {
    public abstract class Reaction : LevelContent {
        
        [Flags]
        public enum Type {
            Move = 1 << 0,
            Match = 1 << 1,
            Gravity = 1 << 2
        }

        public abstract int GetPriority();

        public abstract IEnumerator React();

        public abstract Type GetReactionType();

        public override SpaceObject EmitBody() {
            return null;
        }
        
        public override System.Type GetContentBaseType() {
            return typeof (Reaction);
        }
    }
}