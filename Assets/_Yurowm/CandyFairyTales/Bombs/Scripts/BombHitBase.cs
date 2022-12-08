using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yurowm.ContentManager;
using Yurowm.Serialization;

namespace YMatchThree.Core {
    public abstract class BombHitBase: ISerializable {
        
        protected SlotModule epicenter {get; private set;}
        protected LiveContext context {get; private set;}

        public void Initialize(SlotModule epicenter, LiveContext context) {
            this.epicenter = epicenter;
            this.context = context;
        }
        
        public abstract IEnumerator Logic();
        public virtual IEnumerable GetDestroyingEffectCallbacks() {
            yield break;
        }

        public virtual void Serialize(Writer writer) { }

        public virtual void Deserialize(Reader reader) { }
    }
}