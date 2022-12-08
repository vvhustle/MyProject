using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Yurowm.ContentManager;
using Yurowm.Extensions;
using Yurowm.Spaces;
using Yurowm.Utilities;

namespace YMatchThree.Core {
    [RequireComponent (typeof (MeshRenderer))]
    [RequireComponent (typeof (MeshFilter))]
    [DisallowMultipleComponent]
    public class SlotRendererLegacy : SpaceObject {

        Mesh mesh = null;
        public Material material;
        public bool rewriteMaterial = false;
        
        public Color color = Color.white;

        public SortingLayerAndOrder sorting;

        List<int2> slots = new List<int2>();
        public float offsetLeft = 0;
        public float offsetRight = 0;
        public float offsetTop = 0;
        public float offsetBottom = 0;

        public float sizeLeft = 0;
        public float sizeRight = 0;
        public float sizeTop = 0;
        public float sizeBottom = 0;
        
        public bool innerMesh = true;

        public Rect oTL;
        public Rect oTR;
        public Rect oBL;
        public Rect oBR;

        public Rect iTL;
        public Rect iTR;
        public Rect iBL;
        public Rect iBR;

        public Rect BL;
        public Rect BR;
        public Rect BB;
        public Rect BT;

        MeshFilter _meshFilter = null;
        public MeshFilter meshFilter {
            get {
                if (!_meshFilter)
                    _meshFilter = GetComponent<MeshFilter>();
                return _meshFilter;
            }
        }

        MeshRenderer _meshRenderer = null;

        public MeshRenderer meshRenderer {
            get {
                if (!_meshRenderer)
                    _meshRenderer = GetComponent<MeshRenderer>();
                return _meshRenderer;
            }
        }
        
        public void AddSlots(IEnumerable<int2> slots) {
            foreach (var slot in slots)
                AddSlot(slot);
        }
        
        public void AddSlot(int2 slot) {
            if (slots.Contains(slot)) 
                return;
            
            slots.Add(slot);
            Rebuild();
        }
        
        public void Rebuild(IEnumerable<int2> slots) {
            this.slots.Clear();
            this.slots.AddRange(slots);
            Rebuild();
        }
        
        [ContextMenu("Test")]
        void Test() {
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

        bool isDirty = false;
        
        void Update() {
            if (!isDirty) return;
            
            RebuildImmediate();
        }

        public void Rebuild() {
            isDirty = true;
        }
        
        public void RebuildImmediate() {
            if (mesh == null) {
                mesh = new Mesh();
                mesh.name = name + "_SRMesh";
            }
            mesh.Clear();
            
            var cells = new Dictionary<int2, Cell>();

            var points = new List<Point>();
            var faces = new List<Face>();
            var borders = new List<Border>();

            foreach (int2 slot in slots) {
                Cell cell = new Cell();
                
                cell.sortID = - slot.Y * 100 + slot.X;
                cells.Add(slot, cell);
                cell.coord = slot;
                cell.near = 0;
                
                Sides.all
                    .Where(s => slots.Contains(slot + s))
                    .ForEach(s => cell.near |= s);
                
                Face face = new Face();
                face.sortID = cell.sortID;
                cell.face = face;
                faces.Add(face);
                face.visible = innerMesh;
                
                Point point;

                //Bottom Left
                point = new Point(Slot.Offset * slot.X, Slot.Offset * slot.Y);
                points.Add(point);
                face.bl = point;

                //Bottom Right
                point = new Point(Slot.Offset * (1 + slot.X), Slot.Offset * slot.Y);
                points.Add(point);
                face.br = point;

                //Top Left
                point = new Point(Slot.Offset * slot.X, Slot.Offset * (1 + slot.Y));
                points.Add(point);
                face.tl = point; 

                //Top Right
                point = new Point(Slot.Offset * (1 + slot.X), Slot.Offset * (1 + slot.Y));
                points.Add(point);
                face.tr = point; 
            }

            var vertexes = points
                .Select(x => x.position)
                .Distinct()
                .ToDictionaryValue(p => new Point(p.x, p.y));
            
            points.Clear();
            points.AddRange(vertexes.Values);
            
            foreach (Face face in faces) {
                face.bl = vertexes[face.bl.position];
                face.br = vertexes[face.br.position];
                face.tl = vertexes[face.tl.position];
                face.tr = vertexes[face.tr.position];
            }

            if (offsetLeft > 0 || offsetRight > 0 || offsetTop > 0 || offsetBottom > 0) {
                foreach (Cell cell in cells.Values) {
                    if (cell.near.HasFlag(Side.TopRight) && !cell.near.HasFlag(Side.Right) && !cell.near.HasFlag(Side.Top)) {
                        Face face = cell.face;
                        Point point = face.tr.Clone();
                        points.Add(point);
                        face.tr = point;
                        cells[cell.coord + Side.TopRight].near &= ~Side.BottomLeft;
                        cell.near &= ~Side.TopRight;
                    }
                    if (cell.near.HasFlag(Side.TopLeft) && !cell.near.HasFlag(Side.Left) && !cell.near.HasFlag(Side.Top)) {
                        Face face = cell.face;
                        Point point = face.tl.Clone();
                        points.Add(point);
                        face.tl = point;
                        cells[cell.coord + Side.TopLeft].near &= ~Side.BottomRight;
                        cell.near &= ~Side.TopLeft;
                    }
                }
            }

            foreach (Cell cell in cells.Values) {
                foreach (Side side in Sides.all)
                    if (!cell.near.HasFlag(side)) {
                        if (side.IsSlanted())
                            if (cell.near.HasFlag(side.Rotate(1)) != cell.near.HasFlag(side.Rotate(-1)))
                                continue;
                        Border border = new Border();
                        border.outer = !cell.near.HasFlag(side.Rotate(1));
                        border.cell = cell;
                        border.side = side;
                        borders.Add(border);
                    }
            }

            if (offsetLeft == 0 && offsetRight == 0 && offsetTop == 0 && offsetBottom == 0) {
                foreach (Cell cell in cells.Values) {
                    if (cell.near.HasFlag(Side.TopRight) && !cell.near.HasFlag(Side.Right) && !cell.near.HasFlag(Side.Top)) {
                        Border border = new Border();
                        border.cell = new Cell();
                        border.cell.coord = cell.coord + Side.Right;
                        border.cell.face = new Face();
                        border.cell.face.sortID = cell.sortID;

                        border.cell.face.tl = cell.face.tr;
                        border.side = Side.TopLeft;
                        border.outer = false;
                        borders.Add(border);

                        border = new Border();
                        border.cell = new Cell();
                        border.cell.coord = cell.coord + Side.Top;
                        border.cell.face.sortID = cell.sortID;
                        border.cell.face = new Face();
                        border.cell.face.br = cell.face.tr;
                        border.side = Side.BottomRight;
                        border.outer = false;
                        borders.Add(border);
                    }
                    if (cell.near.HasFlag(Side.TopLeft) && !cell.near.HasFlag(Side.Left) && !cell.near.HasFlag(Side.Top)) {
                        Border border = new Border();
                        border.cell = new Cell();
                        border.cell.coord = cell.coord + Side.Left;
                        border.cell.face.sortID = cell.sortID;
                        border.cell.face = new Face();
                        border.cell.face.tr = cell.face.tl;
                        border.side = Side.TopRight;
                        border.outer = false;
                        borders.Add(border);

                        border = new Border();
                        border.cell = new Cell();
                        border.cell.coord = cell.coord + Side.Top;
                        border.cell.face.sortID = cell.sortID;
                        border.cell.face = new Face();
                        border.cell.face.bl = cell.face.tl;
                        border.side = Side.BottomLeft;
                        border.outer = false;
                        borders.Add(border);
                    }
                }
            }

            foreach (Side side in Sides.straight) {
                List<Point> p = borders.Where(x => x.side == side).SelectMany(x => x.cell.face.GetBorder(side)).Distinct().ToList();
                foreach (Point point in p) {
                    switch (side) {
                        case Side.Left: point.position.x += Slot.Offset * offsetLeft; break;
                        case Side.Right: point.position.x -= Slot.Offset * offsetRight; break;
                        case Side.Top: point.position.y -= Slot.Offset * offsetTop; break;
                        case Side.Bottom: point.position.y += Slot.Offset * offsetBottom; break;
                    }
                }
            }

            List<Face> borderFaces = new List<Face>();
            List<Point> borderPoints = new List<Point>();

            foreach (Side side in Sides.slanted) {
                if (BorderSize(side.Horizontal()) == 0 || BorderSize(side.Vertical()) == 0) continue;
            
                foreach (Border b in borders.Where(x => x.side == side)) {
                    Point p = b.cell.face.GetPoint(side);
                    if (p == null) continue;
                    Face face = new Face();
                    face.sortID = b.cell.sortID;
                    borderFaces.Add(face);
                    Vector2 size = new Vector2();
                    size.x = side.X() > 0 ? sizeRight : sizeLeft;
                    size.y = side.Y() > 0 ? sizeTop : sizeBottom;
                    size *= Slot.Offset;
                    Vector2 position = p.position;
                    position.x += 0.5f * size.x * side.X();
                    position.y += 0.5f * size.y * side.Y();

                    Rect uv;

                    if (b.outer)
                        uv = OuterCornerRect(side);
                    else
                        uv = InnerCornerRect(side);


                    Point point;
                    //Bottom Left
                    point = new Point(position.x - size.x / 2, position.y - size.y / 2);
                    borderPoints.Add(point);
                    point.uv2.x = uv.xMin;
                    point.uv2.y = uv.yMin;
                    face.bl = point;

                    //Bottom Right
                    point = new Point(position.x + size.x / 2, position.y - size.y / 2);
                    borderPoints.Add(point);
                    point.uv2.x = uv.xMax;
                    point.uv2.y = uv.yMin;
                    face.br = point;

                    //Top Left
                    point = new Point(position.x - size.x / 2, position.y + size.y / 2);
                    borderPoints.Add(point);
                    point.uv2.x = uv.xMin;
                    point.uv2.y = uv.yMax;
                    face.tl = point;

                    //Top Right
                    point = new Point(position.x + size.x / 2, position.y + size.y / 2);
                    borderPoints.Add(point);
                    point.uv2.x = uv.xMax;
                    point.uv2.y = uv.yMax;
                    face.tr = point;
                }
            }

            foreach (Side side in Sides.straight) {
                if (BorderSize(side) == 0) continue;
            
                List<Border> b = borders.Where(x => x.side == side).ToList();
                foreach (Border _b in b) {
                    Face face = new Face();
                    face.sortID = _b.cell.sortID;
                    borderFaces.Add(face);


                    Rect uv = BorderRect(side.Mirror());

                    var _p = _b.cell.face.GetBorder(_b.side).ToArray();

                    Vector2 size = new Vector2();
                    size.x = side.Y() != 0 ? Mathf.Abs(_p[0].position.x - _p[1].position.x) : (side.X() > 0 ? sizeRight : sizeLeft) * Slot.Offset;
                    size.y = side.X() != 0 ? Mathf.Abs(_p[0].position.y - _p[1].position.y) : (side.Y() > 0 ? sizeTop : sizeBottom) * Slot.Offset;

                    Vector2 position = new Vector2();
                    position.x = (_p[0].position.x + _p[1].position.x) / 2;
                    position.y = (_p[0].position.y + _p[1].position.y) / 2;
                    if (side.Y() != 0) position.y += (side.Y() > 0 ? sizeTop : -sizeBottom) * Slot.Offset / 2;
                    else position.x += (side.X() > 0 ? sizeRight : -sizeLeft) * Slot.Offset / 2;

                    Point point;
                    //Bottom Left
                    point = new Point(position.x - size.x / 2, position.y - size.y / 2);
                    borderPoints.Add(point);
                    point.uv2.x = uv.xMin;
                    point.uv2.y = uv.yMin;
                    face.bl = point;

                    //Bottom Right
                    point = new Point(position.x + size.x / 2, position.y - size.y / 2);
                    borderPoints.Add(point);
                    point.uv2.x = uv.xMax;
                    point.uv2.y = uv.yMin;
                    face.br = point;

                    //Top Left
                    point = new Point(position.x - size.x / 2, position.y + size.y / 2);
                    borderPoints.Add(point);
                    point.uv2.x = uv.xMin;
                    point.uv2.y = uv.yMax;
                    face.tl = point;

                    //Top Right
                    point = new Point(position.x + size.x / 2, position.y + size.y / 2);
                    borderPoints.Add(point);
                    point.uv2.x = uv.xMax;
                    point.uv2.y = uv.yMax;
                    face.tr = point;

                    for (int r = -1; r <= 1; r += 2) {
                        if (_b.cell.near.HasFlag(side.Rotate(r))) {
                            _p = face.GetBorder(side.Rotate(r * 2)).ToArray();
                            foreach (Point k in _p) {
                                switch (side.Rotate(r * 2)) {
                                    case Side.Left: k.position.x += Slot.Offset * sizeRight; break;
                                    case Side.Right: k.position.x -= Slot.Offset * sizeLeft; break;
                                    case Side.Top: k.position.y -= Slot.Offset * sizeBottom; break;
                                    case Side.Bottom: k.position.y += Slot.Offset * sizeTop; break;
                                }
                            }
                        }
                    }
                }
            }
       
            faces.AddRange(borderFaces);

            foreach (Point point in points)
                point.uv2 = Vector2.left;
            points.AddRange(borderPoints);

            foreach (Point point in points) {
                point.uv1.x = point.position.x / Slot.Offset;
                point.uv1.y = point.position.y / Slot.Offset;
            }

            int id = 0;
            points.ForEach(x => x.ID = id++);
            mesh.SetVertices(points.Select(x => x.position).ToList());
            mesh.SetColors(points.Select(p => color).ToArray());
            
            faces.Sort((x, y) => x.sortID.CompareTo(y.sortID));

            mesh.SetTriangles(faces.Where(f => f.visible).SelectMany(x => x.GetIDs()).ToArray(), 0);
            mesh.SetUVs(0, points.Select(x => x.uv1).ToArray());
            mesh.SetUVs(1, points.Select(x => x.uv2).ToArray());

            mesh.RecalculateNormals();

            meshFilter.mesh = mesh;
            if (rewriteMaterial)
                meshRenderer.material = material;
            meshRenderer.sortingLayerID = sorting.layerID;
            meshRenderer.sortingOrder = sorting.order;
            
            isDirty = false;
        }

        public void Clear() {
            slots.Clear();
            if (mesh == null) {
                mesh = new Mesh();
                mesh.name = name + "_SRMesh";
            }
            mesh.Clear();
            meshFilter.mesh = mesh;
        }

        float BorderSize(Side side) {
            switch (side) {
                case Side.Right: return sizeRight;
                case Side.Left: return sizeLeft;
                case Side.Top: return sizeTop;
                case Side.Bottom: return sizeBottom;
            }
            return 0;
        }

        Rect BorderRect(Side side) {
            switch (side) {
                case Side.Right: return BR;
                case Side.Left: return BL;
                case Side.Top: return BT;
                case Side.Bottom: return BB;
            }
            throw new Exception("The side is not straight");
        }

        Rect InnerCornerRect(Side side) {
            switch (side) {
                case Side.TopRight: return iTL;
                case Side.TopLeft: return iTR;
                case Side.BottomRight: return iBL;
                case Side.BottomLeft: return iBR;
            }
            throw new Exception("The side is straight");
        }

        Rect OuterCornerRect(Side side) {
            switch (side) {
                case Side.TopRight: return oBR;
                case Side.TopLeft: return oBL;
                case Side.BottomRight: return oTR;
                case Side.BottomLeft: return oTL;
            }
            throw new Exception("The side is straight");
        }

        class Cell {
            public int2 coord;
            public Face face;
            public Side near;
            public int sortID = 0;
        }

        class Face {
            public int sortID = 0;

            public Point bl;
            public Point br;
            public Point tl;
            public Point tr;
            public bool visible = true;

            public Triangle triangleA => new Triangle {
                a = br,
                b = bl,
                c = tl
            };

            public Triangle triangleB => new Triangle {
                a = tl,
                b = tr,
                c = br
            };

            public IEnumerable<int> GetIDs() {
                foreach (var ID in triangleA.GetIDs())
                    yield return ID;
                foreach (var ID in triangleB.GetIDs())
                    yield return ID;
            }

            public IEnumerable<Point> GetBorder(Side side) {
                switch (side) {
                    case Side.Left: {
                        yield return bl;
                        yield return tl;
                        break;
                    }
                    case Side.Right: {
                        yield return br;
                        yield return tr;
                        break;
                    }
                    case Side.Top: {
                        yield return tr;
                        yield return tl;
                        break;
                    }
                    case Side.Bottom: {
                        yield return bl;
                        yield return br;
                        break;
                    }
                }
            }

            public Point GetPoint(Side side) {
                switch (side) {
                    case Side.TopLeft: return tl;
                    case Side.TopRight: return tr;
                    case Side.BottomLeft: return bl;
                    case Side.BottomRight: return br;
                }
                return null;
            }

        }

        class Triangle {
            public Point a;
            public Point b;
            public Point c;

            public IEnumerable<int> GetIDs() {
                yield return a.ID;
                yield return b.ID;
                yield return c.ID;
            }
        }

        class Point {
            public Vector3 position;
            public int ID;
            public Vector2 uv1;
            public Vector2 uv2;
            
            public Point(float x, float y) {
                position = new Vector3(x, y, 0);
            }

            internal Point Clone() {
                return (Point) MemberwiseClone();
            }
        }

        class Border {
            public Cell cell;
            public Side side;
            public bool outer;
        }
    }
}