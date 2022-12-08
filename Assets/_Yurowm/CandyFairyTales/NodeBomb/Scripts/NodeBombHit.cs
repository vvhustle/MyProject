using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Yurowm.Effects;
using Yurowm.Extensions;
using Yurowm.Serialization;
using Yurowm.Utilities;
using HitAction = System.Action<Yurowm.Effects.Effect, YMatchThree.Core.Slot, YMatchThree.Core.HitContext>;
using NodeEffectSetup = System.Func<System.Collections.Generic.IEnumerable<Yurowm.Effects.IEffectCallback>>;

namespace YMatchThree.Core {
    public class NodeBombHit : BombHitBase {
        
        public ExplosionParameters explosion = new ExplosionParameters();
        
        public string nodeEffect;
        
        HitAction onReachTarget = null;
        NodeEffectSetup nodeEffectSetup = null;
        
        List<Slot> targets;
        YRandom random;
        
        List<Slot> hitGroup = new List<Slot>();
        HitContext hitContext;

        public void Prepare(SlotContent[] targets, HitAction onReachTarget, NodeEffectSetup nodeEffectSetup, YRandom random) {
            this.onReachTarget = onReachTarget;
            this.nodeEffectSetup = nodeEffectSetup;
            this.random = random;

            hitGroup = epicenter.Slots().ToList();
            
            this.targets = targets
                .Select(t => t.slotModule.Slot())
                .Distinct()
                .Shuffle(random)
                .ToList();
            
            hitGroup.AddRange(this.targets);
            
            hitContext = new HitContext(context, hitGroup.Distinct(), HitReason.BombExplosion);
        }
        
        public override IEnumerator Logic() {
            
            var slots = context.GetArgument<Slots>();
            var field = slots.field;
            var simulation = field.simulation;
            var position = epicenter.Position;

            try {
                field.Highlight(hitContext.group, epicenter.Slot());
            }
            catch {
                Debug.Log("");
            }
            
            if (!nodeEffect.IsNullOrEmpty() && simulation.AllowEffects()) {
                var wait = 0;
                
                List<IEffectCallback> callbacks = new List<IEffectCallback>();
                
                foreach (var target in targets) {
                    var _target = target;
                    
                    Effect effect = null;
                    
                    callbacks.Reuse(nodeEffectSetup());
                    
                    callbacks.Add(new CompleteCallback {
                        onComplete = () => wait --
                    });
                    
                    wait ++;
                    
                    callbacks.Add(new NodeEffectItemProvider(target) {
                        onReachTarget = () => {
                            field.Explode(_target.position, explosion); 
                            onReachTarget(effect, _target, hitContext);
                        }
                    });
                    
                    effect = Effect.Emit(field, nodeEffect, position, callbacks.ToArray());
                    
                    field.AddContent(effect);
                    
                    if (simulation.AllowToWait())
                        yield return null;
                }
                
                while (wait != 0)
                    yield return null;
                
            } else {
                targets.ForEach(t => field.Explode(t.position, explosion));
                targets.ForEach(t => onReachTarget(null, t, hitContext));
            }
        }

        public override void Serialize(Writer writer) {
            base.Serialize(writer);
            
            writer.Write("explosion", explosion);
            writer.Write("nodeEffect", nodeEffect);
        }

        public override void Deserialize(Reader reader) {
            base.Deserialize(reader);
            
            reader.Read("explosion", ref explosion);
            reader.Read("nodeEffect", ref nodeEffect);
        }
    }
}