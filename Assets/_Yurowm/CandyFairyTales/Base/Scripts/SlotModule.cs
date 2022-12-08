using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Yurowm;
using Yurowm.Extensions;
using Yurowm.Utilities;

namespace YMatchThree.Core {

    public abstract class SlotModule : Module {
            
        public SlotModule() {}
        
        public abstract int2 Center { get; }
        public abstract Vector2 Position { get; }

        public abstract IEnumerable<Slot> Slots();
        
        public virtual IEnumerable<C> GetContent<C>() where C : SlotContent {
            return Slots().SelectMany(s => s.GetAllContent<C>());
        }

        public virtual bool HasSlot() {
            return Slots().HasElements();
        }
        
        public virtual bool Has(Slot slot) {
            return Slots().Contains(slot);
        }

        public abstract void SetCenterSlot(Slot slot);
        
        public abstract void Remove(Slot slot);


        public void AddContent(SlotContent content) {
            Slots().ForEach(s => s.AddContent(content));
        }
        
        public bool HasBaseContent<C>() where C : SlotContent {
            return Slots().Any(s => s.HasBaseContent<C>());
        }

        public bool HasContent<C>() where C : SlotContent {
            return Slots().Any(s => s.HasContent<C>());
        }

        public virtual Slot Slot() {
            return Slots().FirstOrDefault();
        }

    }
    
    public class SingleSlotModule : SlotModule {
        
        public SingleSlotModule() {}

        Slot slot;
        
        public override Slot Slot() => slot;

        public override int2 Center => slot.coordinate;
        public override Vector2 Position => slot?.position ?? Vector2.zero;

        public override IEnumerable<Slot> Slots() {
            if (slot)
                yield return slot;
        }

        public override IEnumerable<C> GetContent<C>() {
            var result = slot?.GetContent<C>();
            if (result)
                yield return result; 
        }

        public override bool HasSlot() {
            return slot != null;
        }

        public override bool Has(Slot slot) {
            return this.slot == slot;
        }

        public override void SetCenterSlot(Slot slot) {
            this.slot = slot;
        }

        public override void Remove(Slot slot) {
            if (this.slot == slot)
                this.slot = null;
        }
    }
    
    public class MultipleSlotModule : SlotModule {
        public override int2 Center => Slot()?.coordinate ?? int2.Null;
        
        public override Vector2 Position => Slot()?.position ?? Vector2.zero;
        
        List<Slot> slots = new List<Slot>();
        
        public override IEnumerable<Slot> Slots() {
            return slots;
        }

        public override Slot Slot() {
            return slots.Count > 0 ? slots[0] : null;
        }

        public override void SetCenterSlot(Slot slot) {
            slots.Clear();
    
            slots.Add(slot);
            
            if (gameEntity is IBigSlotContent bigSlotContent && gameEntity is SlotContent slotContent) {
                var slotContainer = gameEntity.context.GetArgument<Slots>();
                
                foreach (var coord in bigSlotContent.GetBigShape().GetCoords()) {
                    if (slotContainer.all.TryGetValue(slot.coordinate + coord, out var s) && !slots.Contains(s)) {
                        slots.Add(s);
                        s.LinkContent(slotContent);
                    }
                }
            }
        }

        public override void Remove(Slot slot) {
            slots.Remove(slot);
        }
    }
    
    public class TempSlotModule : SlotModule {
        Slot slot;
        
        public TempSlotModule(Slot slot) {
            this.slot = slot;
        }

        public override int2 Center => slot.coordinate;
        
        public override Vector2 Position => slot.position;
        public override IEnumerable<Slot> Slots() {
            yield return slot;
        }

        public override void SetCenterSlot(Slot slot) {
            Debug.LogError("It is not supported!");
        }

        public override void Remove(Slot slot) {
            Debug.LogError("It is not supported!");
        }
    }
}