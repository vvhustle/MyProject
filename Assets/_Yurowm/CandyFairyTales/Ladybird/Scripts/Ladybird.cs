using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using Yurowm;
using Yurowm.Coroutines;
using Yurowm.Extensions;
using Yurowm.Serialization;
using Yurowm.Utilities;
using Space = Yurowm.Spaces.Space;

namespace YMatchThree.Core {
    public class Ladybird : Ingredient {
        
        int2? lastCoord;
        Side side;
        
        public ExplosionParameters moveExplosion = new ExplosionParameters();

        public override void OnAddToSpace(Space space) {
            base.OnAddToSpace(space);

            Reactions.Get(field).Emit<LadybirdMoveReaction>();

            side = Sides.straight.GetRandom(random, "InitialSide");

            direction = side.ToAngle();
            
            if (body.SetupChildComponent(out SortingGroup sortingGroup))
                sortingGroup.sortingOrder = 5;
        }

        IEnumerator Move(List<Slot> targets) {
            var slot = slotModule.Slot();
                
            var target = slot.nearSlot
                .Where(p => p.Key.IsStraight() && p.Value)
                .Select(p => p.Value)
                .Where(targets.Contains)
                .GetRandom(random);
                
            if (!target) yield break;
                
            targets.Remove(target);
            
            field.Explode(position, moveExplosion);

            var loopClip = lcAnimator.PlayOpenLoopClose(null, "Move", null, 10);
                
            var currentAngle = side.ToAngle();
            side = slot.nearSlot.GetKey(target);
            var targetAngle = side.ToAngle(); 
                
            for (var t = 0f; t < 1f; t += Time.deltaTime / .3f) {
                    
                direction = Mathf.LerpAngle(currentAngle, targetAngle, EasingFunctions.InCubic(t));
                    
                yield return null;
            }
                
            direction = targetAngle;
                
            target.GetCurrentContent().HideAndKill();

            for (var t = 0f; t < 1f; t += Time.deltaTime / .8f) {
                    
                position = Vector2.Lerp(slot.position, target.position, EasingFunctions.OutCubic(t));
                    
                yield return null;
            }
                
            target.AddContent(this);
            lastCoord = target.coordinate;
            
            yield return lcAnimator.StopAndWait(loopClip);
            
            lcAnimator.animator.RewindEnd("Awake");
        }
        
        public override void OnChangeSlot() {
            base.OnChangeSlot();
            if (!lastCoord.HasValue)
                lastCoord = slotModule.Center;
        }

        protected override IEnumerator Collecting() {
            var loopClip = lcAnimator.PlayOpenLoopClose(
                null, "Move", null, 10);
            
            var currentAngle = side.ToAngle();
            
            var targetAngle = 90f + random.Range(30f, 60f) * random.Sign(); 
            
            for (var t = 0f; t < 1f; t += Time.deltaTime / .3f) {
                    
                direction = Mathf.LerpAngle(currentAngle, targetAngle, EasingFunctions.InCubic(t));
                    
                yield return null;
            }
            
            field.Explode(position, moveExplosion);
            
            BreakParent();
            
            yield return lcAnimator.StopAndWait(loopClip);
            
            loopClip = lcAnimator.PlayOpenLoopClose(
                null, "Fly", null, 10);

            if (body.SetupChildComponent(out SortingGroup sortingGroup))
                sortingGroup.sortingOrder = 1000;
            
            for (var t = 0f; t < 4f; t += Time.deltaTime) {
                
                direction = targetAngle + YMath.SinRad(t * 8) * 30f;
                position += Vector2.right.Rotate(direction) * (3f * Time.deltaTime);
                
                yield return null;
            }
            
            yield return lcAnimator.StopAndWait(loopClip);
        }

        
        public class LadybirdMoveReaction : Reaction {
            public override int GetPriority() {
                return 3;
            }

            public override IEnumerator React() {
                var ladybirds = context.GetAll<Ladybird>(l => l.slotModule.HasSlot()).ToList();
                
                if (ladybirds.Count == 0) {
                    Kill();
                    yield break;
                }
                
                ladybirds.RemoveAll(l => {
                    if (l.lastCoord.HasValue && l.lastCoord != l.slotModule.Center) {
                        l.lastCoord = l.slotModule.Center;
                        return true;
                    }
                    return false;
                });
                
                if (ladybirds.Count == 0)
                    yield break;

                var targets = ladybirds
                    .SelectMany(l => l.slotModule.Slot().nearSlot
                        .Where(p => p.Key.IsStraight() && p.Value && 
                                    p.Value.GetCurrentContent() is Chip c &&
                                    c is IDestroyable))
                    .Select(p => p.Value)
                    .Distinct()
                    .ToList();
                
                targets.RemoveAll(t => t.GetCurrentContent() is Ladybird);
                
                yield return ladybirds
                    .Select(l => l.Move(targets))
                    .Parallel();
                
                yield return Reactor.Result.CompleteToGravity;
            }

            public override Type GetReactionType() {
                return Type.Move;
            }
        }

        #region ISerializable

        public override void Serialize(Writer writer) {
            base.Serialize(writer);
            writer.Write("moveExplosion", moveExplosion);
        }

        public override void Deserialize(Reader reader) {
            base.Deserialize(reader);
            reader.Read("moveExplosion", ref moveExplosion);
        }

        #endregion
        
    }
}