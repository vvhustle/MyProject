using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Yurowm.Serialization;
using Yurowm.Utilities;
using Space = Yurowm.Spaces.Space;

namespace YMatchThree.Core {
    [SerializeShort]
    public class RealisticChipPhysic : ChipPhysic {
        List<Slot> straightSlots = new List<Slot>();
        StraightChipPhysic straightPhysic = new StraightChipPhysic();

        Slots slots;

        public override void OnAddToSpace(Space space) {
            base.OnAddToSpace(space);
            slots = context.GetArgument<Slots>();
        }

        public override Slot GetFallingSlot(Slot slot) {
            Slot target = straightPhysic.GetFallingSlot(slot);
            if (target) return target;

            target = slot[Side.Top];
            bool topLocked = target && target.HasBaseContent<Chip>() && !target.HasBaseContent<Block>();

            Side lockerSide = Side.Left;
            Side targetSide = Side.BottomLeft;
            target = slot[targetSide];
            var lockedSlot = slot[lockerSide];
            if (target && IsAvailableForFalling(target))
                if (!lockedSlot || lockedSlot.HasBaseContent<Block>() || !topLocked && !straightSlots.Contains(target)) 
                    return target;

            lockerSide = Side.Right;
            targetSide = Side.BottomRight;
            target = slot[targetSide];
            lockedSlot = slot[lockerSide];
            if (target && IsAvailableForFalling(target))
                if (!lockedSlot || lockedSlot.HasBaseContent<Block>() || !topLocked && !straightSlots.Contains(target)) 
                    return target;

            return null;
        }
        
        List<Slot> closed = new List<Slot>();
        List<Slot> opened = new List<Slot>();

        public override void OnStartGravity() {
            closed.Clear();
            opened.Clear();
            straightSlots.Clear();
            opened.AddRange(slots.all.Values.Where(s => !s.HasBaseContent<Block>() && s.HasBaseContent<ChipGenerator>()));
            straightSlots.AddRange(opened);

            while (opened.Count > 0) {
                var current = opened[0];
                opened.RemoveAt(0);
                closed.Add(current);
                foreach (Slot f in current.falling.Values) {
                    if (f && !f.HasBaseContent<Block>() && !straightSlots.Contains(f) && !opened.Contains(f) && !closed.Contains(f)) {
                        straightSlots.Add(f);
                        opened.Add(f);
                    }
                }
            }
        }
    }
    
}