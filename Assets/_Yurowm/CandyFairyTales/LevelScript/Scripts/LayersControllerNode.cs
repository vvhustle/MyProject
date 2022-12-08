using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Yurowm;
using Yurowm.ContentManager;
using Yurowm.Coroutines;
using Yurowm.Extensions;
using Yurowm.Serialization;
using Yurowm.Utilities;

namespace YMatchThree.Core {
    public class LayersControllerNode : ActionNode {

        public List<LayerAction> layers = new List<LayerAction>();
        
        public override IEnumerator Logic(object[] args) {
            var field = args?.CastOne<Field>();
            
            if (!field || field.complete)
                yield break;
            
            var gameplay = field.fieldContext.Get<LevelGameplay>();

            gameplay.shuffleLockers.Add(this);

            using (var task = gameplay.NewExternalTask()) {
                yield return task.WaitAccess();
                
                var level = field.fieldContext.GetArgument<Level>();
                var slots = field.slots;
                
                var newSlots = new Dictionary<int2, SlotInfo>(); 
                var slotsToHide = new Dictionary<int2, Slot>();

                foreach (var action in layers) {
                    var layer = level.layers.FirstOrDefault(l => l.ID == action.layerID);

                    if (layer == null) continue;

                    switch (action.state) {
                        case LayerState.Show: {
                            foreach (var s in layer.GetSlots(level))
                                if (!slots.all.ContainsKey(s.coordinate) && !newSlots.ContainsValue(s))
                                    newSlots.Add(s.coordinate, s);
                        } break;
                        case LayerState.Hide: {
                            foreach (var s in layer.GetSlots(level))
                                if (!slotsToHide.ContainsKey(s.coordinate) && slots.all.TryGetValue(s.coordinate, out var slot))
                                    slotsToHide.Add(s.coordinate, slot);
                        } break;
                        case LayerState.OnlyThis: {
                            slotsToHide = slots.all.ToDictionary();
                            newSlots.Clear();
                            foreach (var s in layer.GetSlots(level)) {
                                if (slots.all.ContainsKey(s.coordinate))
                                    slotsToHide.Remove(s.coordinate);
                                else
                                    newSlots.Add(s.coordinate, s);
                            }
                        } break;
                    }
                }

                var animations = new List<IEnumerator>();

                var curretSlotsBody = slots.body as SlotsBody;
                curretSlotsBody.Rebuild(slots.all.Keys.Where(c => !slotsToHide.ContainsKey(c)).ToArray());

                if (newSlots.Count > 0) {
                    newSlots.Values
                        .Where(s => {
                            if (!slots.hidden.TryGetValue(s.coordinate, out var slot)) return true;
                            slots.hidden.Remove(s.coordinate);
                            slots.all.Add(s.coordinate, slot);
                            slot.enabled = true;
                            return false;
                        })
                        .ToDictionaryKey(field.CreateSlot)
                        .ForEach(p => field.FillSlot(p.Key, p.Value));

                    var newSlotsBody = slots.EmitBody() as SlotsBody;
                    
                    newSlotsBody.transform.SetParent(slots.body.transform.parent);
                    newSlotsBody.transform.Reset();
                    newSlotsBody.Rebuild(newSlots.Keys.ToArray());
                    
                    animations.Add(newSlotsBody.Show().ContinueWith(() => {
                        curretSlotsBody.Rebuild(slots.all.Keys.ToArray());
                        newSlotsBody.Kill();
                    }));
                }

                if (slotsToHide.Count > 0) {
                    foreach (var pair in slotsToHide) {
                        slots.hidden.Add(pair.Key, pair.Value);
                        slots.all.Remove(pair.Key);
                    }

                    var killSlotsBody = slots.EmitBody() as SlotsBody;
                    
                    killSlotsBody.transform.SetParent(slots.body.transform.parent);
                    killSlotsBody.transform.Reset();
                    killSlotsBody.Rebuild(slotsToHide.Keys.ToArray());
                    
                    animations.Add(killSlotsBody.Hide().ContinueWith(killSlotsBody.Kill));
                    animations.AddRange(slotsToHide.Values.Select(s => s.Hiding()));
                }

                yield return animations.Parallel();

                field.slots.Bake();

                gameplay.shuffleLockers.Remove(this);
                
                yield return field.RefreshPositionSmooth();
                
                gameplay.NextTask<GravityTask>();
                
                Push(outputPort, field);
            }
        }
        
        public enum LayerState {
            Show = 0,
            Hide = 1,
            OnlyThis = 2
        }

        [SerializeShort]
        public class LayerAction : ISerializable {
            public string layerID;
            public LayerState state;
            
            public void Serialize(Writer writer) {
                writer.Write("ID", layerID);
                writer.Write("state", state);
            }

            public void Deserialize(Reader reader) {
                reader.Read("ID", ref layerID);
                reader.Read("state", ref state);
            }
        }
        
        #region ISerializable
        
        public override void Serialize(Writer writer) {
            base.Serialize(writer);
            writer.Write("layers", layers.ToArray());
        }

        public override void Deserialize(Reader reader) {
            base.Deserialize(reader);
            layers.Reuse(reader.ReadCollection<LayerAction>("layers"));
        }

        #endregion
    }
}