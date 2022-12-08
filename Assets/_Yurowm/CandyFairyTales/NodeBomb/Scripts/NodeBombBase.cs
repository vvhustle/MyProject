using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Yurowm.ContentManager;
using Yurowm.Coroutines;
using Yurowm.Effects;
using Yurowm.Extensions;
using Yurowm.Serialization;
using Yurowm.Utilities;

namespace YMatchThree.Core {
    public abstract class NodeBombBase : BombChipBase {
        public NodeBombHit hit = new NodeBombHit();
        
        public override IEnumerator Exploding() {
            hit.Initialize(slotModule, context);
            hit.Prepare(
                targets: GetNodeTargets()
                    .Where(t => t != this)
                    .ToArray(),
                onReachTarget: OnReachedTheTarget,
                nodeEffectSetup: NodeEffectSetup,
                random: random);
            yield return hit.Logic();
        }
        
        protected virtual IEnumerable<IEffectCallback> NodeEffectSetup() {
            yield break;
        }

        protected abstract IEnumerable<SlotContent> GetNodeTargets();
        
        public abstract void OnReachedTheTarget(Effect effect, Slot target, HitContext hitContext);

        #region ISerializable

        public override void Serialize(Writer writer) {
            base.Serialize(writer);
            
            writer.Write("hit", hit);
        }

        public override void Deserialize(Reader reader) {
            base.Deserialize(reader);
            
            reader.Read("hit", ref hit);
        }

        #endregion
    }
}