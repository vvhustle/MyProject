using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Yurowm;
using Yurowm.Colors;
using Yurowm.Extensions;
using Yurowm.Shapes;
using Yurowm.Utilities;

namespace YMatchThree.Core {
    public class SlotRenderer : Shape2DBehaviour, IOnAnimateHandler, IRepaint32Target {

        #region Properties
        
        [SerializeField]
        Color m_Color = new Color(255, 255, 255, 255);
        
        public Color32 Color {
            get => m_Color;
            set {
                if (Equals(m_Color, value)) return;
                m_Color = value;   
                SetDirty();
            }
        }
        
        [SerializeField]
        float m_Offset;

        public float Offset {
            get => m_Offset;
            set {
                if (m_Offset == value) return;
                m_Offset = value;   
                SetDirty();
            }
        }
        
        [SerializeField]
        float m_OuterCornerRadius = .1f;
        
        public float OuterCornerRadius {
            get => m_OuterCornerRadius;
            set {
                if (m_OuterCornerRadius == value) return;
                m_OuterCornerRadius = value;   
                SetDirty();
            }
        }
        
        [SerializeField]
        float m_InnerCornerRadius = .1f;
        
        public float InnerCornerRadius {
            get => m_InnerCornerRadius;
            set {
                if (m_InnerCornerRadius == value) return;
                m_InnerCornerRadius = value;   
                SetDirty();
            }
        }
        
        [SerializeField]
        int m_OuterCornerDetails = 5;
        
        public int OuterCornerDetails {
            get => m_OuterCornerDetails;
            set {
                if (m_OuterCornerDetails == value) return;
                m_OuterCornerDetails = value;   
                SetDirty();
            }
        }

        [SerializeField]
        int m_InnerCornerDetails = 5;
        
        public int InnerCornerDetails {
            get => m_InnerCornerDetails;
            set {
                if (m_InnerCornerDetails == value) return;
                m_OuterCornerDetails = value;   
                SetDirty();
            }
        }
        
        #endregion
        
        [UnityEngine.ContextMenu("Test/Shape")]
        void TestShape() {
            Rebuild(new List<int2>() {
                new int2(0, 0),
                new int2(1, 0),
                new int2(2, 0),
                new int2(3, 0),
                new int2(0, 1),
                new int2(2, 1),
                new int2(3, 1),
                new int2(0, 2),
                new int2(1, 2),
                new int2(2, 2),
                new int2(3, 2),
                new int2(1, 3)
            });
        }
        [UnityEngine.ContextMenu("Test/Line")]
        void TestLine() {
            Rebuild(new List<int2>() {
                new int2(0, 0),
                new int2(1, 0),
                new int2(2, 0),
                new int2(3, 0),
                new int2(4, 0)
            });
        }

        float borderSize = 0;
        public bool fromSlotCenter = false;
        
        public MeshBuilderBase.MeshOptimization optimizeMesh = 0;

        public bool slotBaseUV = false;
        
        public override void FillMesh(MeshBuilder builder) {
            
            GenerateSlotHash();
            
            m_OuterCornerRadius = Mathf.Clamp(m_OuterCornerRadius, 0, 
                Mathf.Min(Slot.Offset - m_Offset * 2, Slot.Offset - m_Offset * 2) / 2);
            m_InnerCornerRadius = Mathf.Clamp(m_InnerCornerRadius, 0, 
                Mathf.Min(Slot.Offset - m_Offset * 2, Slot.Offset - m_Offset * 2) / 2);
            
            borderSize = Mathf.Max(m_InnerCornerRadius / 4, m_OuterCornerRadius / 2);
            
            Dictionary<int2, InnerMeshFace> innerMesh = new Dictionary<int2, InnerMeshFace>();
            List<BorderFace> borders = new List<BorderFace>();
            
            // Generete Inner Meshes
            foreach (var slot in GetSlots().Distinct()
                .OrderBy(s => s.X + s.Y * 10000)) {
                
                innerMesh.Add(slot, new InnerMeshFace(slot) {
                    fill = GetFillAtPoint(slot)
                });
            }
            
            // Generete Inner Mesh Points
            foreach (var face in innerMesh.Values) {
                
                if (face.fill.HasFlag(Side.Right)) {
                    var nextFace = innerMesh[face.coordRight];
                    Face.PairPoints(face, Side.TopRight, nextFace, Side.TopLeft);
                    Face.PairPoints(face, Side.BottomRight, nextFace, Side.BottomLeft);
                }                
                
                if (face.fill.HasFlag(Side.Top)) {
                    var nextFace = innerMesh[face.coordTop];
                    Face.PairPoints(face, Side.TopRight, nextFace, Side.BottomRight);
                    Face.PairPoints(face, Side.TopLeft, nextFace, Side.BottomLeft);
                }
                
                if (face.bl == null) face.bl = new Point();
                if (face.br == null) face.br = new Point();
                if (face.tl == null) face.tl = new Point();
                if (face.tr == null) face.tr = new Point();
                
                face.bl.position = CoordToVector(face.coord);
                face.br.position = CoordToVector(face.coordRight);
                face.tl.position = CoordToVector(face.coordTop);
                face.tr.position = CoordToVector(face.coordTopRight);
            }
            
            float _offset = m_Offset + borderSize;
            
            // Move Inner Mesh Points
            foreach (var face in innerMesh.Values) {
                var fill = face.fill;
                
                if (!fill.HasFlag(Side.Right)) {
                    face.br.offset.x = -_offset;
                    face.tr.offset.x = -_offset;
                }
                if (!fill.HasFlag(Side.Left)) {
                    face.bl.offset.x = _offset;
                    face.tl.offset.x = _offset;
                }
                if (!fill.HasFlag(Side.Top)) {
                    face.tl.offset.y = -_offset;
                    face.tr.offset.y = -_offset;
                }
                if (!fill.HasFlag(Side.Bottom)) {
                    face.bl.offset.y = _offset;
                    face.br.offset.y = _offset;
                }
            }
            
            // Generete Borders
            if (borderSize > 0) {
                foreach (var side in Sides.straight) {
                    foreach (var face in innerMesh.Values) {
                        if (face.fill.HasFlag(side)) continue;
                        var border = new BorderFace {
                            innerMesh = face,
                            side = side
                        };

                        var sideB = side.Mirror();
                        
                        Face.PairPoints(face, side.Rotate(1), border, sideB.Rotate(-1));
                        Face.PairPoints(face, side.Rotate(-1), border, sideB.Rotate(1));
                        
                        sideB = side.Rotate(1);
                        
                        Point point = face.GetPoint(sideB).Clone();
                        point.position += side.ToVector2() * borderSize;
                        border.SetPoint(sideB, point);
                        
                        sideB = side.Rotate(-1);
                        
                        point = face.GetPoint(sideB).Clone();
                        point.position += side.ToVector2() * borderSize;
                        border.SetPoint(sideB, point);
                                
                        borders.Add(border);
                    }
                }
                
                foreach (var side in Sides.slanted) {
                    foreach (var face in innerMesh.Values) {
                        var sideLeft = side.Rotate(1);
                        var sideRight = side.Rotate(-1);
                        if (!face.fill.HasFlag(sideLeft) && !face.fill.HasFlag(sideRight)) {
                            var borderLeft = borders.FirstOrDefault(b => b.innerMesh == face && b.side == sideLeft);
                            var borderRight = borders.FirstOrDefault(b => b.innerMesh == face && b.side == sideRight);
                            AddOuterCorener(builder, face.GetPoint(side), side, borderLeft, borderRight);
                        }    
                        else if (!face.fill.HasFlag(side) && face.fill.HasFlag(sideLeft) && face.fill.HasFlag(sideRight)) {
                            var f = innerMesh[face.coord + sideLeft];
                            var borderLeft = borders.FirstOrDefault(b => b.innerMesh == f && b.side == sideRight);
                            f = innerMesh[face.coord + sideRight];
                            var borderRight = borders.FirstOrDefault(b => b.innerMesh == f && b.side == sideLeft);
                            AddInnerCorener(builder, face.GetPoint(side), side, borderLeft, borderRight);
                        }
                    }
                }
            }
            
            // Mesh Generating:
            innerMesh.Values.ForEach(f => f.Fill(builder, m_Color));
            borders.ForEach(f => f.Fill(builder, m_Color));
            
            builder.Optimize(optimizeMesh);
            if (slotBaseUV)
                builder.GenerateUV((bound, position) => {
                    if (fromSlotCenter)
                        position += Vector2.one * (Slot.Offset / 2);
                    return position / Slot.Offset;
                });
            else { 
                var size = 0f;
                builder.GenerateUV((bound, position) => {
                    if (size <= 0) 
                        size = YMath.Max(-bound.x, bound.x + bound.width, -bound.y, bound.y + bound.height);
                    return position / size;
                });
            }
        }

        #region Slots

        // public SlotsProvider slots;
        
        public void Rebuild(IEnumerable<int2> slots) {
            points.Clear();
            points.AddRange(slots);
            SetDirty();
        }
        public void RebuildImmediate(IEnumerable<int2> slots) {
            points.Clear();
            points.AddRange(slots);
            RebuildImmediate();
        }
        
        public void AddPoints(IEnumerable<int2> p) {
            foreach (var point in p) {
                if (!points.Contains(point))
                    points.Add(point);
            }
            SetDirty();
        }
        
        List<int2> points = new List<int2>();
        
        IEnumerable<int2> GetSlots() {
            // return slots.slots;
            return points;
        }

        #endregion
        
        #region Slot Hash

        int[] slotHash;
        
        void GenerateSlotHash() {
            slotHash = GetSlots().Select(s => s.X * 10000 + s.Y).Distinct().ToArray();
        }
        
        bool IsThereSlot(int x, int y) {
            return slotHash.Contains(x * 10000 + y);
        }
        
        bool IsThereSlot(int2 coord) {
            return IsThereSlot(coord.X, coord.Y);
        }
        
        #endregion
        
        Vector2 CoordToVector(int2 coord) {
            return CoordToVector(coord.X, coord.Y);
        }
        
        Vector2 CoordToVector(int x, int y) {
            if (fromSlotCenter)
                return new Vector2(Slot.Offset * (1f * x - 0.5f), Slot.Offset * (1f * y - 0.5f));
            else
                return new Vector2(Slot.Offset * x, Slot.Offset * y);
        } 
        
        void AddVert(MeshBuilder builder, Vector2 position) {
            builder.AddVert(position, m_Color,
                Vector2.zero, Vector2.zero, 
                Vector3.back, new Vector4(1, 0, 0, -1)); 
        }

        public void OnAnimate() {
            RebuildImmediate();
        }

        class Point {
            public Vector2 position;
            public Vector2 offset;
            
            public Vector2 Position {
                get => position + offset;
                set => position = value - offset;
            }

            public Point() : this(0, 0) {}
            
            public Point(float x, float y) : this(new Vector2(x, y))  { }
            
            public Point(Vector2 position) {
                this.position = position;
            }

            public Point Clone() {
                return (Point) MemberwiseClone();
            }
        }

        #region Faces
        
        class InnerMeshFace : Face {
            public Side fill;
            public readonly int2 coord;
            public int2 coordRight => coord + Side.Right;
            public int2 coordTop => coord + Side.Top;
            public int2 coordTopRight => coord + Side.TopRight;

            public InnerMeshFace(int2 coord) {
                this.coord = coord;
            }
        }
        
        class BorderFace : Face {
            public InnerMeshFace innerMesh;
            public Side side;
        }

        abstract class Face {
            
            public Point bl;
            public Point br;
            public Point tl;
            public Point tr;
            
            public Point GetPoint(Side side) {
                switch (side) {
                    case Side.BottomLeft: return bl;
                    case Side.BottomRight: return br;
                    case Side.TopLeft: return tl;
                    case Side.TopRight: return tr;
                    default: return null;
                }
            }
            public void SetPoint(Side side, Point point) {
                switch (side) {
                    case Side.BottomLeft: bl = point; break;
                    case Side.BottomRight: br = point; break;
                    case Side.TopLeft: tl = point; break;
                    case Side.TopRight: tr = point; break;
                }
            }
            
            public void Fill(MeshBuilder builder, Color color) {
                int index = builder.currentVertCount;
                
                builder.AddVert(bl.Position, color);
                builder.AddVert(tl.Position, color);
                builder.AddVert(br.Position, color);
                builder.AddVert(tr.Position, color);

                builder.AddTriangle(index, index + 1, index + 2);
                builder.AddTriangle(index + 1, index + 3, index + 2);
            }
            
            public static void PairPoints(Face faceA, Side sideA, Face faceB, Side sideB) {
                var pointA = faceA.GetPoint(sideA);
                var pointB = faceB.GetPoint(sideB);
                
                if (pointA == null && pointB == null) {
                    pointA = new Point();
                } else if (pointB != null)
                    pointA = pointB;
                
                faceA.SetPoint(sideA, pointA);
                faceB.SetPoint(sideB, pointA);
            }
        }
        
        #endregion

        [Serializable]
        public class SlotsProvider {
            public List<int2> slots;
        }

        #region Fill
        
        Side GetFillAtPoint(int2 point) {
            Side result = Side.Null;

            foreach (var side in Sides.all)
                if (IsThereSlot(point.X + side.X(), point.Y + side.Y()))
                    result |= side;
            
            return result;
        }
        
        #endregion

        #region Corners

        void AddInnerCorener(MeshBuilder builder, Point point, Side side, BorderFace borderLeft, BorderFace borderRight) {
            
            var center = point.Position + side.ToVector2() * (borderSize + m_InnerCornerRadius);
            
            if (m_InnerCornerRadius <= 0) {
                borderLeft.GetPoint(side.Rotate(-2)).Position = center;
                borderRight.GetPoint(side.Rotate(2)).Position = center;
                return;
            }
            
            float startAngle = side.Mirror().Rotate(-1).ToAngle();
            float endAngle = side.Mirror().Rotate(1).ToAngle();
            
            borderLeft.GetPoint(side.Rotate(-2)).Position = center + Vector2.right.Rotate(startAngle) * m_InnerCornerRadius;
            borderRight.GetPoint(side.Rotate(2)).Position = center + Vector2.right.Rotate(endAngle) * m_InnerCornerRadius;
            
            int index = builder.currentVertCount;
            
            AddVert(builder, point.Position);
            
            float step = Mathf.Abs(Mathf.DeltaAngle(startAngle, endAngle) / m_InnerCornerDetails) + .1f;
            
            int counter = 1;
            for (float a = startAngle; a != endAngle; a = Mathf.MoveTowardsAngle(a, endAngle, step)) {
                AddVert(builder, center + Vector2.right.Rotate(a) * m_InnerCornerRadius);
                counter ++;
            }
            
            AddVert(builder, center + Vector2.right.Rotate(endAngle) * m_InnerCornerRadius);
            
            for (int i = 0; i < counter; i++) 
                builder.AddTriangle(index, index + i, index + i + 1);

        }

        void AddOuterCorener(MeshBuilder builder, Point point, Side side, BorderFace borderLeft, BorderFace borderRight) {
            var center = point.Position + side.ToVector2() * (borderSize - m_OuterCornerRadius);
            
            if (m_OuterCornerRadius <= 0) {
                m_OuterCornerRadius = 0;
            
                borderLeft.GetPoint(side).Position = center;
                borderRight.GetPoint(side).Position = center;

                return;
            }
            
            float startAngle = side.Rotate(1).ToAngle();
            float endAngle = side.Rotate(-1).ToAngle();
            
            borderLeft.GetPoint(side).Position = center + Vector2.right.Rotate(startAngle) * m_OuterCornerRadius;
            borderRight.GetPoint(side).Position = center + Vector2.right.Rotate(endAngle) * m_OuterCornerRadius;
            
            int index = builder.currentVertCount;
            
            AddVert(builder, point.Position);

            float step = Mathf.Abs(Mathf.DeltaAngle(startAngle, endAngle) / m_OuterCornerDetails) + .1f;
            
            int counter = 1;
            for (float a = startAngle; a != endAngle; a = Mathf.MoveTowardsAngle(a, endAngle, step)) {
                AddVert(builder, center + Vector2.right.Rotate(a) * m_OuterCornerRadius);
                counter ++;
            }
            
            AddVert(builder, center + Vector2.right.Rotate(endAngle) * m_OuterCornerRadius);
            
            for (int i = 0; i < counter; i++) 
                builder.AddTriangle(index, index + i, index + i + 1);

        }

        

        #endregion
    }
}