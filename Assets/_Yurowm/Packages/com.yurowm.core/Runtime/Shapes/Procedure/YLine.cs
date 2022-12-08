using System.Collections.Generic;
using UnityEngine;
using Yurowm.Extensions;

namespace Yurowm.Shapes {
    public class YLine {
        
        public enum ConnectionType {Thread, Chain, Brick}
        
        List<Vector2> vertices = new List<Vector2>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector2> uv = new List<Vector2>();
        List<Triangle> triangles = new List<Triangle>();
        
        #region Points
        
        List<Vector2> points = new List<Vector2>();
        
        public void AddPoint(Vector2 point) {
            points.Add(point);
        }
        public void SetPoints(IEnumerable<Vector2> points) {
            this.points.Clear();
            this.points.AddRange(points);
        }

        public void ChangePoint(int id, Vector2 point) {
            if (id >= 0 && id < points.Count) 
                points[id] = point;
        }

        public int PointsCount() {
            return points.Count;
        }

        public void Clear() {
            points.Clear();
        }
        
        public float GetLength() {
            if (points.Count < 2) return 0;
            
            var length = 0f;
            
            var previousPoint = points[0];

            for (int i = 1; i < points.Count; i++) {
                var currentPoint = points[i];   
                length += (currentPoint - previousPoint).FastMagnitude();
                previousPoint = currentPoint;
            }
            
            return length;
        }
        
        #endregion
        
        public void FillMesh(MeshBuilderBase builder, Order order) {
            vertices.Clear();
            normals.Clear();
            uv.Clear();
            triangles.Clear();
            
            if (points.Count < 2 || order.thickness <= 0) return;
            
            switch (order.type) {
                case ConnectionType.Thread: BuildThread(order); break;
                case ConnectionType.Chain:
                case ConnectionType.Brick: BuildBrick(order); break;
            }
            
            for (int i = 0; i < vertices.Count; i++) {
                builder.AddVert(vertices[i], order.color, 
                    new Vector2(uv[i].x, uv[i].y), 
                    Vector2.zero, 
                    order.directionNormals ? normals[i] : Vector3.back,
                    new Vector4(1, 0, 0, -1));
            }

            foreach (Triangle triangle in triangles) 
                builder.AddTriangle(triangle.a, triangle.b, triangle.c);
            
        }
        
        void BuildThread(Order order) {
            int p = 0;
            float length = 0;
            ;
            if (order.smooth >= 2 && points.Count >= 3) {
                Vector2 a, b, c, d;
                float t;

                order.points = new List<Vector2>();
                List<Vector2> guides = new List<Vector2>();
                for (int i = 0; i < points.Count; i++) {
                    if (i == 0 || i == points.Count - 1)
                        guides.Add(Vector2.zero);
                    else {
                        Vector2 guide = (points[i - 1] - points[i]).normalized + (points[i + 1] - points[i]).normalized;
                        guide = guide.normalized;
                        guide.x += guide.y;
                        guide.y -= guide.x;
                        guide.x += guide.y;

                        if (Vector2.Angle(guide, points[i + 1] - points[i]) > 90)
                            guide *= -1;
                        guides.Add(guide);
                    }
                }
                for (int i = 0; i < points.Count - 1; i++) {
                    float guide_magnitude = Vector2.Distance(points[i], points[i + 1]) * order.smoothPower;
                    a = points[i];
                    b = a + guides[i] * guide_magnitude;
                    d = points[i + 1];
                    c = d - guides[i + 1] * guide_magnitude;
                    //Debug.DrawLine(points[i], points[i] + guideStart, Color.green, 1);
                    //Debug.DrawLine(points[i + 1], points[i + 1] + guideEnd, Color.red, 1);
                    //Debug.DrawLine(points[i], points[i + 1], Color.yellow, 1);
                    order.points.Add(points[i]);
                    for (int s = 1; s < order.smooth; s++) {
                        t = 1f * s / order.smooth;

                        order.points.Add(Vector2.Lerp(
                            Vector2.Lerp(
                                Vector2.Lerp(a, b, t),
                                Vector2.Lerp(b, c, t),
                                t),
                            Vector2.Lerp(
                                Vector2.Lerp(b, c, t),
                                Vector2.Lerp(c, d, t),
                                t),
                            t
                        ));
                    }
                }
                order.points.Add(points[points.Count - 1]);

            } else
                order.points = points;

            for (int i = 0; i < order.points.Count; i++) {
                Vector2 a = GetPoint(i - 1, ref order) - GetPoint(i, ref order);
                Vector2 b = GetPoint(i + 1, ref order) - GetPoint(i, ref order);

                a = a.normalized;
                b = b.normalized;

                Vector2 offset = Vector2.Lerp(a, b, 0.5f);
                if (offset == Vector2.zero)
                    offset = Quaternion.Euler(0, 0, 90) * a;
            
                offset = offset.normalized;

                var _thickness = order.thickness / Mathf.Cos(Mathf.Deg2Rad * (90 - Vector2.Angle(a, b) / 2));
                Vector3 left = Vector3.Project(Quaternion.Euler(0, 0, 90) * a, offset).FastNormalized() * _thickness / 2;


                vertices.Add(new Vector3(order.points[i].x, order.points[i].y, 0) - left);
                vertices.Add(new Vector3(order.points[i].x, order.points[i].y, 0) + left);

                if (i > 0)
                    length += (order.points[i - 1] - order.points[i]).FastMagnitude();
                uv.Add(new Vector2(1, length));
                uv.Add(new Vector2(0, length));
                
                if (order.directionNormals) {
                    var normal = new Vector3(0, 0, ((b - a).Angle(false) + 90) * Mathf.Deg2Rad);
                    normals.Add(normal);
                    normals.Add(normal);
                }

                if (vertices.Count < 4)
                    continue;

                triangles.Add(new Triangle(p + 2, p + 1, p));
                triangles.Add(new Triangle(p + 1, p + 2, p + 3));

                p += 2;
            }

            if (order.loop) {
                vertices.Add(vertices[0]);
                vertices.Add(vertices[1]);

                triangles.Add(new Triangle(p + 2, p + 1, p));
                triangles.Add(new Triangle(p + 1, p + 2, p + 3));

                length += (order.points[order.points.Count - 1] - order.points[0]).FastMagnitude();
                uv.Add(new Vector2(1, length));
                uv.Add(new Vector2(0, length));

                if (order.directionNormals) {
                    var normal = new Vector3(0, 0, ((order.points[0] - order.points[order.points.Count - 1]).Angle(false) + 90) * Mathf.Deg2Rad);
                    normals.Add(normal);
                    normals.Add(normal);
                }
            }
            
            length /= order.tileY;
            
            for (int i = 0; i < uv.Count; i++)
                uv[i] = new Vector2(uv[i].x, uv[i].y / length);
        }

        void BuildBrick(Order order) {
            int p = 0;
            
            order.points = points;
            
            for (int i = 0; i < points.Count; i++) {
                if (order.type == ConnectionType.Chain && i == 0 && !order.loop)
                    continue;

                if (order.type == ConnectionType.Brick && i % 2 == 0)
                    continue;

                Vector2 a = GetPoint(i - 1, ref order);
                Vector2 b = GetPoint(i, ref order);

                Vector2 offset = Quaternion.Euler(0, 0, 90) * (b - a);
                offset = offset.normalized;
            
                Vector3 left = offset * order.thickness / 2;
                
                vertices.Add(new Vector3(a.x, a.y, 0) + left);
                vertices.Add(new Vector3(a.x, a.y, 0) - left);
                uv.Add(new Vector2(0, 0));
                uv.Add(new Vector2(1, 0));

                vertices.Add(new Vector3(b.x, b.y, 0) + left);
                vertices.Add(new Vector3(b.x, b.y, 0) - left);
                uv.Add(new Vector2(0, 1));
                uv.Add(new Vector2(1, 1));

                triangles.Add(new Triangle(p + 2, p + 1, p));
                triangles.Add(new Triangle(p + 1, p + 2, p + 3));
                p += 4;
            }
        }

        Vector2 GetPoint(int i, ref Order order) {
            var points = order.points;
            
            if (points.Count == 0)
                return Vector2.zero;
            
            if (points.Count == 1)
                return points[0];
            
            if (i < 0) {
                if (order.loop)
                    return points[points.Count - 1];
                return Vector2.LerpUnclamped(points[0], points[1], -1);
            }

            if (i >= points.Count) {
                if (order.loop)
                    return points[0];
                return Vector2.LerpUnclamped(points[points.Count - 1], points[points.Count - 2], -1);
            }
            
            return points[i];
        }
        
        public struct Order {
            public ConnectionType type;
            public Color32 color;
            public bool directionNormals;
            
            public List<Vector2> points;
            
            public bool loop;
            
            public float thickness;
            public float tileY;
            
            public float smooth;
            public float smoothPower;
        }
        
        struct Triangle {
            public int a;
            public int b;
            public int c;

            public Triangle(int a, int b, int c) {
                this.a = a;
                this.b = b;
                this.c = c;
            }
        }
    }
}