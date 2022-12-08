using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yurowm;
using Yurowm.Extensions;
using Yurowm.Shapes;

namespace YMatchThree.Core {
    
    public class LevelMapPointsProvider : UnityEngine.EventSystems.UIBehaviour {
        public Vector2[] points;
        

        public Vector2 GetPoint(int index) {
            if (index < 0 || index >= points.Length) return default;
            
            return TransformPoint(points[index]);
        }
        
        public IEnumerable<Vector2> GetPoints() {
            foreach (var point in points)
                yield return TransformPoint(point);
        }
        
        public Vector3 TransformPoint(Vector2 point) {
            return transform.TransformPoint(point);
        }
        
        public Vector2 InverseTransformPoint(Vector3 point) {
            return transform.InverseTransformPoint(point).To2D();
        }
    }
}