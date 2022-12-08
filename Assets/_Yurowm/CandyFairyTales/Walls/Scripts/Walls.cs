using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Yurowm.ContentManager;
using Yurowm.Extensions;
using Yurowm.Serialization;
using Yurowm.Utilities;
using Space = Yurowm.Spaces.Space;

namespace YMatchThree.Core {
    public class Walls : LevelSlotExtension {
        
        WallsVariable info;

        public string verticalWallBodyName;
        public string horizontalWallBodyName;
        public string cornerBodyName;
        
        Slots slots;
        
        public override void OnAddToSpace(Space space) {
            base.OnAddToSpace(space);
            
            slots = context.GetArgument<Slots>();
            slots.onBaked += Apply;
            
            Apply();
        }

        public override void OnRemoveFromSpace(Space space) {
            base.OnRemoveFromSpace(space);
            slots.onBaked -= Apply;
        }

        void Apply() {
            var vertWall = new List<int2>();
            var horizWall = new List<int2>();
            
            foreach (var slot in slots.all.Values) {
                foreach (var side in Sides.straight) {
                    if (!slots.all.ContainsKey(slot.coordinate + side)) continue;
                    if (WallInfo.IsWall(slot.coordinate, side, slots.all.Keys, info)) {
                        slot.nearSlot[side] = null;
                        if (side == Side.Top) horizWall.Add(slot.coordinate + int2.up);
                        else if (side == Side.Left) vertWall.Add(slot.coordinate);
                    }
                }
            }
            
            foreach (var slot in slots.all.Values) {
                foreach (var side in Sides.slanted) {
                    if (!slots.all.ContainsKey(slot.coordinate + side)) continue;
                    if (!slot[side]) continue;
                    
                    if (!slot[side.Rotate(1)] && !slot[side.Rotate(-1)]) {
                        slot[side] = null;
                        continue;
                    }
                    
                    if (!slot[side.Rotate(1)]?[side.Rotate(-2)] || !slot[side.Rotate(-1)]?[side.Rotate(2)])
                        slot[side] = null;
                }
            }
            
            slots.all.Values.ForEach(x => x.CalculateFallingSlot());
            
            BuildBodies();
        }

        #region Bodies
        
        List<SlotContentBody> bodies = new List<SlotContentBody>();
        
        void BuildBodies() {
            KillBodies();
            
            SlotContentBody Emit(string bodyName) {
                var result = AssetManager.Emit<SlotContentBody>(bodyName, field.fieldContext);
                if (!result) return null;
                result.transform.SetParent(field.root);
                result.transform.Reset();
                bodies.Add(result);
                return result;
            }
            
            if (!verticalWallBodyName.IsNullOrEmpty()) {
                var query = info.orientations
                    .Where(o => o.Value.HasFlag(OrientationLine.Horizontal))
                    .Select(o => o.Key);

                foreach (var coord in query) {
                    var body = Emit(verticalWallBodyName);
                    if (!body) break;
                    body.transform.localPosition = (coord.ToVector2() + new Vector2(0, .5f)) * Slot.Offset;
                }
            }
            
            if (!horizontalWallBodyName.IsNullOrEmpty()) {
                var query = info.orientations
                    .Where(o => o.Value.HasFlag(OrientationLine.Vertical))
                    .Select(o => o.Key);

                foreach (var coord in query) {
                    var body = Emit(horizontalWallBodyName);
                    if (!body) break;
                    body.transform.localPosition = (coord.ToVector2() + new Vector2(.5f, 0)) * Slot.Offset;
                }
            }
            
            if (!cornerBodyName.IsNullOrEmpty()) {
                var corners = new Dictionary<int2, Side>();
                
                void Set(int2 c, Side s) {
                    corners.TryGetValue(c, out var side);    
                    corners[c] = side | s;
                }
                
                foreach (var pair in info.orientations) {
                    if (pair.Value.HasFlag(OrientationLine.Horizontal)) {
                        Set(pair.Key, Side.Top);
                        Set(pair.Key.Up(), Side.Bottom);
                    }

                    if (pair.Value.HasFlag(OrientationLine.Vertical)) {
                        Set(pair.Key, Side.Right);
                        Set(pair.Key.Right(), Side.Left);
                    }
                }

                var visibleSides = new [] {
                    Side.Bottom | Side.Top | Side.Left | Side.Right,
                    Side.Bottom | Side.Left,
                    Side.Bottom | Side.Right,
                    Side.Top | Side.Left,
                    Side.Top | Side.Right,
                    Side.Bottom | Side.Top | Side.Left,
                    Side.Bottom | Side.Top | Side.Right,
                    Side.Bottom | Side.Left | Side.Right,
                    Side.Top | Side.Left | Side.Right
                };
                
                bool FilterCorners(Side sides) { 
                    return visibleSides.Contains(sides);
                }
                
                foreach (var pair in corners.Distinct()) {
                    if (!FilterCorners(pair.Value))
                        continue;
                    
                    var body = Emit(cornerBodyName);
                    if (!body) break;
                    
                    body.transform.localPosition = pair.Key.ToVector2() * Slot.Offset;
                }
                    
            }
        }
        
        void KillBodies() {
            if (bodies.IsEmpty())
                return;
            
            bodies.ForEach(b => b.Kill());
            bodies.Clear();
        }

        #endregion
        
        public override Type GetContentBaseType() {
            return typeof (LevelExtension);
        }
        
        public override void SetupVariable(ISerializable variable) {
            base.SetupVariable(variable);
            switch (variable) {
                case WallsVariable walls: info = walls; return;
            }
        }

        public override IEnumerator GetVariblesTypes() {
            yield return base.GetVariblesTypes();
            yield return typeof(WallsVariable);
        }
        
        #region ISerializable

        public override void Serialize(Writer writer) {
            base.Serialize(writer);
            writer.Write("verticalWallBody", verticalWallBodyName);
            writer.Write("horizontalWallBody", horizontalWallBodyName);
            writer.Write("cornerBody", cornerBodyName);
        }

        public override void Deserialize(Reader reader) {
            base.Deserialize(reader);
            reader.Read("verticalWallBody", ref verticalWallBodyName);
            reader.Read("horizontalWallBody", ref horizontalWallBodyName);
            reader.Read("cornerBody", ref cornerBodyName);
        }

        #endregion
        
        public struct WallInfo {
            public bool exist;
            public int2 coord;
            public OrientationLine orientation;
            
            public static bool IsWall(int2 coord, Side side, ICollection<int2> slots, WallsVariable info) {
                var wall = GetWall(coord, side, slots);
            
                if (!wall.exist) return false;

                if (info.orientations.TryGetValue(wall.coord, out var orientation))
                    return orientation.HasFlag(wall.orientation);

                return false;
            }
        
            public static WallInfo GetWall(int2 coord, Side side, ICollection<int2> slots) {
                var result = new WallInfo();
                
                if (side.IsSlanted() || !slots.Contains(coord + side)) {
                    result.exist = false;
                    return result;
                }
                
                result.exist = true;

                result.coord = coord;
                
                if (side.Y() == 0) {
                    result.orientation = OrientationLine.Horizontal;
                    if (side.X() > 0) result.coord += side;
                } else {
                    result.orientation = OrientationLine.Vertical;
                    if (side.Y() > 0) result.coord += side;
                }

                return result;
            }
        }
    }
    
    public class WallsVariable : ContentInfoVariable, IFieldSensitiveVariable {
        public Dictionary<int2, OrientationLine> orientations = new Dictionary<int2, OrientationLine>();

        public void MoveSlot(int2 from, int2 to) {
            if (orientations.TryGetValue(from, out var value)) {
                orientations[to] = value;
                orientations.Remove(from);
            }
        }

        public void RemoveSlot(int2 from) {
            orientations.Remove(from);
        }
        
        public override void Serialize(Writer writer) {
            writer.Write("orientationKeys", orientations.Keys.ToArray());
            writer.Write("orientationValues", orientations.Values.Cast<int>().ToArray());
        }

        public override void Deserialize(Reader reader) {
            var keys = reader.ReadCollection<int2>("orientationKeys");
            var values = reader.ReadCollection<int>("orientationValues");
            
            orientations.Reuse(keys.Zip(values, (c, o) 
                => new KeyValuePair<int2, OrientationLine>(c, (OrientationLine) o)));
        }
    }
}