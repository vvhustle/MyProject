using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Yurowm.ContentManager;
using Yurowm.Coroutines;
using Yurowm.Extensions;
using Yurowm.Utilities;
using Space = Yurowm.Spaces.Space;

namespace YMatchThree.Core {
    public class Slots : LevelContent {
        public Dictionary<int2, Slot> all = new Dictionary<int2, Slot>();
        public Dictionary<int2, Slot> hidden = new Dictionary<int2, Slot>();
        
        public Rect edge;
        
        public SlotsBody slotsBody;

        public override Type BodyType => typeof(SlotsBody);

        public Action onBaked = delegate {};

        public override Type GetContentBaseType() {
            return typeof (Slots);
        }

        public override void OnEnable() {
            base.OnEnable();
            slotsBody?.Show().Run(space.coroutine);
        }

        public void FirstBake() {
            Bake();
            slotsBody?.Show().Run(space.coroutine);
        }

        public void Bake() {
            all.Values.ForEach(s => s.nearSlot.Clear());
            hidden.Values.ForEach(s => {
                s.nearSlot = s.nearSlot.Keys.ToDictionaryValue(_ => (Slot) null);
                s.falling = s.falling.Keys.ToDictionaryValue(_ => (Slot) null);
            });
            
            foreach (Slot slot in all.Values) {
                foreach (Side side in Sides.all) {
                    all.TryGetValue(slot.coordinate + side, out var nearSlot);
                    slot.nearSlot.Add(side, nearSlot);
                }
                slot.nearSlot.Add(Side.Null, null);
            }
            
            onBaked?.Invoke();
            
            foreach (Slot slot in all.Values) {
                slot.slots = this;   
                slot.CalculateFallingSlot();
            }
            
            slotsBody = body as SlotsBody;
            
            slotsBody?.Rebuild(all.Keys.ToArray());

            CalculateRect();
        }

        public void CalculateRect() {
            if (all.IsEmpty()) {
                edge = new Rect(0, 0, 
                    5 * Slot.Offset, 5 * Slot.Offset);
                return;
            }
            
            var boundDetector = new BoundDetector2D();
            
            all.Values
                .ForEach(s => boundDetector.Set(s.GetRect()));
            
            edge = boundDetector.GetBound();
        }

        public override IEnumerator HidingAndKill() {
            yield return slotsBody?.Hide();
            Kill();
        }

        #region Selection

        public List<Slot> selection = new List<Slot>();
        
        public void Select(Slot slot) {
            if (selection.Contains(slot)) 
                return;
            slot.OnSelect();
            selection.Add(slot);
        }
        
        public void Unselect(Slot slot) {
            if (slot == null || !selection.Contains(slot)) 
                return;
            slot.OnUnselect();
            selection.Remove(slot);
        }
        
        public void Unselect(int index) {
            if (index >= 0 && index < selection.Count) 
                Unselect(selection[index]);
        }
        
        public void ClearSelection() {
            foreach (var slot in selection) 
                slot.OnUnselect();
            selection.Clear();
        }

        #endregion
    }
}