using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Yurowm.Extensions;

namespace YMatchThree.Core {
    public class GravityTask : LevelGameplay.InternalTask {
        
        Slots slots;
        ChipPhysic physic;
        ChipGeneratorController generator;
        
        List<Chip> chips = new List<Chip>();

        public override void OnCreate() {
            base.OnCreate(); 
            slots = gameplay.slots;
            physic = context.Get<ChipPhysic>();
            generator = context.Get<ChipGeneratorController>();
        }

        public override IEnumerator Logic() {
            physic.OnStartGravity();
            
            slots.Bake();
            
            chips.Reuse(slots.all.Values
                .Where(s => s.visible)
                .Select(s => s.GetCurrentContent())
                .CastIfPossible<Chip>());
            
            chips.ForEach(c => c.StartSimulation());
            
            void OnGenerate(Chip chip) {
                chips.Add(chip);
                chip.StartSimulation();
            }

            events.onGenerate += OnGenerate;
            
            var gravity = false;
            
            var lastGeneration = 0f;
            var time = gameplay.time;
            
            bool Generate() {
                if (lastGeneration + .1f > time.AbsoluteTime)
                    return true;
                
                if (generator.Generate()) {
                    lastGeneration = time.AbsoluteTime;
                    return true;
                }
                
                return false;
            }

            while (Generate() || chips.Any(c => !c.IsStable())) {
                gravity = true;
                yield return null;
            }

            events.onGenerate -= OnGenerate;
            
            chips.ForEach(c => c.StopSimulation());
            
            while (chips.Any(c => c.IsSimulating()))
                yield return null;

            if (gravity) 
                Reactions.Get(field).Fill(Reaction.Type.Gravity);

            gameplay.NextTask<MatchingTask>();
        }
    }
}