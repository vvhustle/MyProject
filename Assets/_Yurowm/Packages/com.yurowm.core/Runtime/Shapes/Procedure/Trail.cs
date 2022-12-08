using System.Collections.Generic;
using UnityEngine;
using Yurowm.Extensions;
using Yurowm.Utilities;

namespace Yurowm.Shapes {
    [ExecuteInEditMode]
    [RequireComponent(typeof(YLine2D))]
    public class Trail : BaseBehaviour {
        List<Vector2> points = new List<Vector2>();
        
        public float vertexDistance = .1f;
        public float maxDistance = 3f;
        
        YLine2D line;

        void OnEnable() {
            points.Clear();
        }

        void Update() {
            if (vertexDistance < 0.1f) vertexDistance = 0.1f;
            
            //TODO: Refactor it
            
            float distance = 0;
            
            if (points.Count == 0) {
                points.Add(transform.position);
            } else {
                Vector2 lastPoint = points[points.Count - 1];
                distance = (lastPoint - (Vector2) transform.position).FastMagnitude() - vertexDistance / 10;
                
                while (distance > vertexDistance) {
                    lastPoint = Vector2.MoveTowards(lastPoint, transform.position, vertexDistance);
                    points.Add(lastPoint);
                    distance -= vertexDistance;
                }
            }
            
            if (points.Count >= 3)
                distance += (points.Count - 2) * vertexDistance;

            float endVertexDistance = (GetLastPoint() - GetLastFixedPoint()).FastMagnitude();
            
            distance += endVertexDistance;
            
            while (distance > maxDistance && points.Count > 0) {
                float cropDistance = endVertexDistance > 0 ? endVertexDistance : vertexDistance;
                
                float delta = distance - maxDistance;
                if (delta >= cropDistance) {
                    points.RemoveAt(0);
                    distance -= cropDistance;
                    endVertexDistance = 0;
                } else {
                    var a = GetLastFixedPoint();
                    var b = GetLastPoint();
                    
                    float d = (a - b).FastMagnitude() - delta;
                    
                    points[0] = Vector2.MoveTowards(a, b, d);
                    distance = -delta;
                }
            }
            
            if (!line && !this.SetupComponent(out line)) 
                return;
            
            line.Clear();

            if (points.IsEmpty()) return;
            
            var nearPoint = points[points.Count - 1];
            
            if ((nearPoint - (Vector2) transform.position).MagnitudeIsGreaterThan(vertexDistance / 10))
                line.AddPoint(Vector2.zero);
            else 
                line.AddPoint(transform.InverseTransformPoint(nearPoint));
            
            for (int i = points.Count - 2; i >= 0; i--)
                line.AddPoint(transform.InverseTransformPoint(points[i]));
            
            line.RebuildImmediate();
        }
        
        Vector2 GetLastFixedPoint() {
            return points.Count > 1 ? points[1] : transform.position.To2D();
        }
        
        Vector2 GetLastPoint() {
            return points.Count > 0 ? points[0] : transform.position.To2D();
        }
    }
}