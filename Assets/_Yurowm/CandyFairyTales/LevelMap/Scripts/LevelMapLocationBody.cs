using UnityEngine;
using YMatchThree.Core;
using Yurowm;
using Yurowm.ContentManager;
using Yurowm.Spaces;
using Yurowm.Utilities;

namespace YMatchThree.Seasons {
    public class LevelMapLocationBody : SpaceObject, IReserved {
        public LevelMapPointsProvider pointsProvider;

        public float top = 5;
        public float bottom = 0;
        
        [Min(0)]
        public float visibilityOffsetTop = 0;
        [Min(0)]
        public float visibilityOffsetBottom = 0;
        
        public const float Width = 10;
        
        FloatRange _edges = 0;
        FloatRange _visibleEdges = 0;
        
        public FloatRange Edges {
            get {
                _edges.min = bottom;
                _edges.max = top;
                return _edges;
            }
        }
        
        public FloatRange VisibleEdges {
            get {
                _visibleEdges.min = bottom - visibilityOffsetBottom.ClampMin(0);
                _visibleEdges.max = top + visibilityOffsetTop.ClampMin(0);
                return _visibleEdges;
            }
        }
        
        public float Length => Edges.Interval;
        
        public void Rollout() { }

        void OnDrawGizmos () {
            Gizmos.color = Color.yellow;
            
            Gizmos.matrix = transform.localToWorldMatrix;
            
            Gizmos.DrawLine(
                new Vector3(-Width/2, bottom, 0), 
                new Vector3(-Width/2, top, 0));
            Gizmos.DrawLine(
                new Vector3(Width/2, bottom, 0), 
                new Vector3(Width/2, top, 0));
            
            Gizmos.DrawLine(
                new Vector3(-Width/2, bottom, 0), 
                new Vector3(Width/2, bottom, 0));
            Gizmos.DrawLine(
                new Vector3(-Width/2, top, 0), 
                new Vector3(Width/2, top, 0));
            
            Gizmos.color = Color.magenta;
            
            if (visibilityOffsetBottom > 0) {
                Gizmos.DrawLine(
                    new Vector3(-Width/2, bottom - visibilityOffsetBottom, 0), 
                    new Vector3(Width/2, bottom - visibilityOffsetBottom, 0));
                
                Gizmos.DrawLine(
                    new Vector3(-Width/2, bottom - visibilityOffsetBottom, 0), 
                    new Vector3(-Width/2, bottom, 0));
                Gizmos.DrawLine(
                    new Vector3(Width/2, bottom - visibilityOffsetBottom, 0), 
                    new Vector3(Width/2, bottom, 0));
            }
                
            if (visibilityOffsetTop > 0) {
                Gizmos.DrawLine(
                    new Vector3(-Width/2, top + visibilityOffsetTop, 0), 
                    new Vector3(Width/2, top + visibilityOffsetTop, 0));
                
                Gizmos.DrawLine(
                    new Vector3(-Width/2, top + visibilityOffsetTop, 0), 
                    new Vector3(-Width/2, top, 0));
                Gizmos.DrawLine(
                    new Vector3(Width/2, top + visibilityOffsetTop, 0), 
                    new Vector3(Width/2, top, 0));
            }
        }
    }
}