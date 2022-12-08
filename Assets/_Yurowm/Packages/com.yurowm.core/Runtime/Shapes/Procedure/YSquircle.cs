using System.Collections.Generic;
using UnityEngine;

namespace Yurowm.Shapes {
    public class YSquircle {

        List<Vector2> vertices = new List<Vector2>();

        public void FillMesh(MeshBuilderBase builder, Order order) {
            if (order.size.x <= 0 || order.size.y <= 0)
                return;
            
            vertices.Clear();
            
            
            float step = (360f / order.details);
            
            var power = new Vector2(
                (order.size.y / order.size.x).ClampMax(1),
                (order.size.x / order.size.y).ClampMax(1)) *
                        order.power;
            
            var offset = (Vector2.one * .5f - order.pivot) * order.size;
            
            vertices.Add(offset);
            
            for (int i = 0; i < order.details; i++) {
                var angle = step * i;
                var vertex = new Vector2(YMath.CosDeg(angle), YMath.SinDeg(angle));
                vertex.x = Mathf.Pow(vertex.x.Abs(), power.x) * Mathf.Sign(vertex.x) / 2;
                vertex.y = Mathf.Pow(vertex.y.Abs(), power.y) * Mathf.Sign(vertex.y) / 2;
                
                vertex.Scale(order.size);
                vertex += offset;
                vertices.Add(vertex);
            }
                
            foreach (var vertex in vertices) 
                builder.AddVert(vertex, order.color);

            builder.AddTriangle(0, 1, order.details);
            for (int i = 1; i < order.details; i++) 
                builder.AddTriangle(0, i + 1, i);
        }
        
        public struct Order {
            public Color32 color;
            public int details;
            public float power;
            public Vector2 size;
            public Vector2 pivot;
        }
    }
}