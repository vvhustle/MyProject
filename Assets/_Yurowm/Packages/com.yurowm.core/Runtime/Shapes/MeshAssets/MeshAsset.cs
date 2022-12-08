using System;
using System.Collections.Generic;
using UnityEngine;
using Yurowm.Extensions;

namespace Yurowm.Shapes {
    
    using TransformMode = MeshUtils.TransformMode;
    
    public class MeshAsset : ScriptableObject {
        
        public float Scale = 1f;
        
        [HideInInspector]
        public Vector2[] vertices;
        
        [HideInInspector]
        public int[] triangles;
        
        [HideInInspector]
        public Vector2[] uv0;
        
        [HideInInspector]
        public Vector2[] uv1;

        [HideInInspector]
        public Border[] borders;
        
        #region Asset Creating

        public static MeshAsset Create(Mesh mesh) {
            var asset = CreateInstance<MeshAsset>();
            
            asset.GenerateMeshAsset(mesh);
            
            return asset;
        }

        #endregion
        
        #region Mesh Building
        
        public struct Order {
            public IShapeBuilder builder;
            public Color32 color;
            public Vector2[] vertices;
            public TransformMode transformMode;
            public Options options;
            
            internal bool flip;
            
            [Flags]
            public enum Options {
                Antialising = 1 << 1,
                DynimicBorderDirections = 1 << 2,
            }
            
            public Order(IShapeBuilder builder) {
                this.builder = builder;
                color = new Color32(255, 255, 255, 255);
                vertices = null;
                transformMode = 0;
                options = 0;
                flip = false;
            }
        }
        
        public void BuildMesh(Order order) { 
            int currentCount = order.builder.currentVertCount;

            if (order.vertices == null || order.vertices.Length != vertices.Length)
                order.vertices = vertices;

            for (int i = 0; i < order.vertices.Length; i++)
                order.builder.AddVert(order.vertices[i], order.color,
                    uv0[i], uv1[i], 
                    Vector3.back, new Vector4(1, 0, 0, -1));
            
            for (int i = 0; i < triangles.Length; i += 3)
                order.builder.AddTriangle(
                    currentCount + triangles[i],
                    currentCount + triangles[i + 1],
                    currentCount + triangles[i + 2],
                    order.flip);

            if (order.options.HasFlag(Order.Options.Antialising))
                BuildMeshAntialiasing(order);
        }

        void BuildMeshAntialiasing(Order order) {
            float antialiasingSize = order.builder.GetPointSize();
            
            if (antialiasingSize <= 0) return;
            
            int initialCount = order.builder.currentVertCount - order.vertices.Length;
            
            bool dynimicBorderDirection = order.options.HasFlag(Order.Options.DynimicBorderDirections);

            foreach (var border in borders) {
                int currentCount = order.builder.currentVertCount;
                
                for (int i = 0; i < border.Length; i++) {
                    var index = border.points[i];
                    var direction = dynimicBorderDirection ?
                        MeshUtils.CulculateBorderDirection(order.vertices, border, i) : 
                        MeshUtils.GetTransformVertex(order.transformMode, border.directions[i]);
                    
                    if (dynimicBorderDirection && order.flip)
                        direction *= -1;
                    
                    order.builder.AddVert(order.vertices[index] + direction * antialiasingSize,
                        order.color.Transparent(0),
                        uv0[index], uv1[index], 
                        Vector3.back, new Vector4(1, 0, 0, -1));
                }
                
                for (int i = 1; i <= border.points.Length; i++) {
                    int c = i == border.points.Length ? 0 : i;
                    int p = i - 1;
                    
                    var indexC = initialCount + border.points[c];
                    var indexP = initialCount + border.points[p];
                    var indexAC = currentCount + c;
                    var indexAP = currentCount + p;
                    
                    order.builder.AddTriangle(indexP, indexAP, indexC, order.flip);
                    order.builder.AddTriangle(indexC, indexAP, indexAC, order.flip);
                }
            }
        }
        
        #endregion
        
        
        public IEnumerable<Vector2> GetTransformVertices(TransformMode mode) {
            foreach (var vertex in vertices)
                yield return MeshUtils.GetTransformVertex(mode, vertex) * Scale;
        }
        
        [Serializable]
        public class Border {
            public int[] points;
            public Vector2[] directions;
            public int Length => points.Length;
        }
        
  
        
    }
    
    public interface IMeshAssetComponent {
        MeshAsset meshAsset {get; set;}
        
        
    }
    
}