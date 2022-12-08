using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yurowm;
using Yurowm.Coroutines;
using Yurowm.DebugTools;
using Yurowm.Extensions;
using Yurowm.Utilities;
using Space = Yurowm.Spaces.Space;

namespace YMatchThree.Core {
    
    [BaseContentOrder(1)]
    public abstract class Chip : SlotContent {

        public override Type GetContentBaseType() {
            return typeof (Chip);
        }

        public override bool IsUniqueContent() {
            return true;
        }

        public bool falling;
        
        ChipPhysic physic;
        
        Transform icon;

        public override void OnAddToSpace(Space space) {
            physic = context.Get<ChipPhysic>();
            
            base.OnAddToSpace(space);
            
            icon = body?.transform.Find("Icon");
        }

        public override void OnRemoveFromSpace(Space space) {
            base.OnRemoveFromSpace(space);
            icon?.Reset();
        }

        public bool toLand = true;
        public bool lockPosition = false;

        public virtual void OnLand() {
            lcAnimator.PlayClip("Landing");
            
            if (physic.bouncing)
                field.Explode(position, physic.landBouncing);
        }
        
        Slot GetNextSlot() {
            if (!IsAlive())
                return null;
            
            var slot = slotModule.Slot();

            if (!slot.IsAlive() || slot.HasBaseContent<Block>())
                return null;

            if (slot.falling.Count == 0)
                return null;
            
            return physic?.GetFallingSlot(slot); 
        }
        
        bool GravityReaction () {
            var nextSlot = GetNextSlot();
            
            if (!nextSlot) 
                return false; 
                
            toLand = true;
            
            nextSlot.AddContent(this);
            
            NewWaypoint();
                
            try {
                GravityReaction();
            } catch (StackOverflowException) {
                Debug.LogError(
                    "Slots directions are in the loop. " 
                    + "Try to not make loops with the Gravitation extension." 
                    + "The loop will be torn in this place.");
                slotModule.Slot().falling.Clear();
                return false;
            } 
            
            return true;
        }
        
        public bool IsStable() {
            return GetNextSlot() == null;
        }
       
        IEnumerator simulationProcess;
        bool simulationAllowed = false;
        
        public void StartSimulation() {
            if (IsSimulating() || field == null) return;
            simulationProcess = SimulationProcess();
            simulationProcess.Run(field.coroutine);
        }
        
        public bool IsSimulating() {
            return simulationProcess != null && IsAlive();
        }
        
        public void StopSimulation() {
            simulationAllowed = false;
        }
        
        IEnumerator SimulationProcess() {
            falling = true;
            
            simulationAllowed = true;
            
            var destroyable = this as IDestroyable;
            
            var slotLocker = context.GetArgument<SlotLocker>();

            if (slotLocker == null) {
                slotLocker = new SlotLocker();
                context.SetArgument(slotLocker);
            }

            float velocity = 0;
            
            Slot waypoint = null;

            while (simulationAllowed && IsAlive()) {
                yield return null;
        
                if (!enabled || !gameplay.IsPlaying() || destroyable != null && destroyable.destroying)
                    continue;
                
                if (!slotModule.HasSlot())
                    yield break;
                
                if (GravityReaction()) {
                    var previousWaypoint = waypoint;
                    waypoint = fallingPath.Grab();
                    if ((position - waypoint.position).MagnitudeIsLessThan(Slot.Offset * 1.5f))
                        yield return slotLocker.Locking(waypoint);
                    slotLocker.Unlock(previousWaypoint);
                }
                
                //fallingPath.Clear();
                
                while (!localPosition.IsEmpty()) {
                    falling = true;
                    
                    while (lockPosition)
                        yield return null;
        
                    if (destroyable != null && destroyable.destroying)
                        break;

                    var deltaDistance = 0f;
                    
                    if (simulation.AllowAnimations()) {
                        if (velocity == 0) velocity = physic.speedInitial;

                        velocity += physic.acceleration * time.Delta;

                        if (velocity > physic.speedMax)
                            velocity = physic.speedMax;
                        
                        deltaDistance = velocity * time.Delta;
                    } else {
                        velocity = 100;
                        deltaDistance = 10;
                    }

                    if (!GravityReaction() && localPosition.MagnitudeIsLessThan(deltaDistance)) {

                        if (simulation.AllowAnimations())
                            while (localPosition.MagnitudeIsGreaterThan(.001f)) {
                                localPosition = Vector2.MoveTowards(localPosition, Vector2.zero,
                                    time.Delta * velocity);
                                yield return null;
                            }
                        else
                            localPosition = Vector2.zero;

                        falling = false;
                        velocity = 0f;

                        if (toLand) {
                            OnLand();
                            toLand = false;
                        }

                        break;
                    }

                    while (deltaDistance > 0) {
                        if (waypoint == null)
                            waypoint = slotModule.Slot();

                        var offset = position - waypoint.position;

                        var moveVector = offset;

                        if (offset.x.Abs() > 0.1f) {
                            if (offset.x < 0) moveVector.x = 1;
                            if (offset.x > 0) moveVector.x = -1;
                        }

                        if (offset.y.Abs() > 0.1f) {
                            if (offset.y < 0) moveVector.y = 1;
                            if (offset.y > 0) moveVector.y = -1;
                        }

                        moveVector = moveVector.FastNormalized() * deltaDistance;

                        if (offset.MagnitudeIsLessThan(moveVector)) {
                            position = waypoint.position;
                            if (fallingPath.Count == 0) {
                                deltaDistance = 0;
                                slotLocker.Unlock(waypoint);
                                waypoint = null;
                            } else {
                                var previousWaypoint = waypoint;
                                waypoint = fallingPath.Grab();
                                
                                if ((position - waypoint.position).MagnitudeIsLessThan(Slot.Offset * 1.5f)) {
                                    if (!slotLocker.Lock(waypoint)) {
                                        velocity = 0;
                                        yield return slotLocker.Locking(waypoint);
                                    }
                                }

                                slotLocker.Unlock(previousWaypoint);
                            }
                        } else
                            position = position.MoveByPath(waypoint.position, ref deltaDistance);
                    }

                    yield return null;
                    falling = false;
                }
                
                falling = false;

                if (waypoint)
                    slotLocker.Unlock(waypoint);
            }
            
            simulationProcess = null;
        }
        
        #region Waypoints
        
        List<Slot> fallingPath = new List<Slot>();
        
        public void NewWaypoint() {
            fallingPath.Add(slotModule.Slot());
        }
        
        class SlotLocker {
            List<Slot> lockedSlots = new List<Slot>();
            
            public bool Lock(Slot slot) {
                if (lockedSlots.Contains(slot))
                    return false;
                lockedSlots.Add(slot);
                return true;
            } 
            
            public void Unlock(Slot slot) {
                lockedSlots.Remove(slot);
            }
            
            public IEnumerator Locking(Slot slot) {
                while (!Lock(slot))
                    yield return null;
            }
        }
        
        #endregion

        #region Bouncing

        Vector2 impuls;

        public void AddImpuls(Vector2 value) {
            if (!IsAlive() || !physic.bouncing || !icon || !simulation.AllowAnimations()) 
                return;
            
            impuls += value;
            
            if (impuls.MagnitudeIsGreaterThan(physic.impulsMax))
                impuls = impuls.WithMagnitude(physic.impulsMax);

            if (bouncing == null) {
                bouncing = Bouncing();
                bouncing.Run(space.coroutine);
            }
        }
        
        IEnumerator bouncing;

        IEnumerator Bouncing() {
            var time = space.time;
            if (icon) {
                while (!impuls.IsEmpty()) {
                    float threshold = Slot.Offset / 2;

                    var p = icon.localPosition.To2D();
                    
                    p += impuls * time.Delta;
                    p *= (1f - time.Delta).Clamp01();
                    impuls -= impuls * time.Delta;
                    impuls -= p * 90f * time.Delta;
                    impuls *= (1f - time.Delta * 6f).Clamp01();

                    if (p.MagnitudeIsLessThan(ChipPhysic.offsetThreshold * time.Delta))
                        if (impuls.MagnitudeIsLessThan(physic.impulsMin))
                            impuls = Vector3.zero;

                    icon.localPosition = p;
                    
                    yield return null;
                }
            }
            icon.localPosition = Vector3.zero;
            
            bouncing = null;
        }
            
        #endregion
    }
    
    public interface IShuffled {}
}