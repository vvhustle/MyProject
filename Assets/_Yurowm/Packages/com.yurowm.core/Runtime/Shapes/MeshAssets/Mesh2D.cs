using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Yurowm.Colors;
using Yurowm.Extensions;
using Yurowm.Utilities;

namespace Yurowm.Shapes {
    public class Mesh2D : Shape2DBehaviour, IRepaintTarget, IMeshAssetComponent, IMeshEffectTarget {

        public MeshAsset meshAsset;
        
        MeshAsset IMeshAssetComponent.meshAsset {
            get => meshAsset;
            set {
                if (meshAsset != value) {
                    meshAsset = value;
                    SetDirty();
                }
            }
        }
        
        [SerializeField]
        Color m_Color = Color.white;
        
        public Color Color {
            get => m_Color;
            set {
                if (m_Color == value) return;
                m_Color = value;
                SetDirty();
            }
        }

        public RectOffsetFloat borders;
        public float scale = 1f;
        
        public MeshUtils.TransformMode transformMode = 0;
        public MeshUtils.Scaling scalingMode = MeshUtils.Scaling.FitInside;
        public MeshAsset.Order.Options options = MeshAsset.Order.Options.Antialising;
        public MeshBuilderBase.UVGenerator uvGenerator = 0;
        
        Vector2[] vertices = null;

        void OnRectTransformDimensionsChange() {
            SetDirty();
        }

        void OnDidApplyAnimationProperties() {
            SetDirty();
        }

        public override void FillMesh(MeshBuilder builder) {
            if (meshAsset == null || meshAsset.vertices.Length < 3) return;
            
            if (vertices == null || vertices.Length != meshAsset.vertices.Length)
                vertices = meshAsset
                    .GetTransformVertices(transformMode)
                    .ToArray();
            else {
                int i = 0;
                meshAsset
                    .GetTransformVertices(transformMode)
                    .ForEach(v => vertices[i++] = v);
            }

            if (transform is RectTransform rectTransform) {
                if (!MeshUtils.TransformVertices(rectTransform, scalingMode, borders, scale, vertices))
                    return;
            } else
                MeshUtils.TransformVertices(scale, vertices);
            
            var order = new MeshAsset.Order(builder) {
                color = m_Color,
                vertices = vertices,
                transformMode = transformMode,
                options = options,
                flip = transformMode.HasFlag(MeshUtils.TransformMode.FlipHorizontal) != 
                       transformMode.HasFlag(MeshUtils.TransformMode.FlipVertical)
            };
            
            foreach (var effect in effects) 
                if (effect.isVisible && effect.Order == MeshEffectOrder.Below)
                    effect.BuildMesh(meshAsset, order);
            
            meshAsset.BuildMesh(order);
            
            foreach (var effect in effects) 
                if (effect.isVisible && effect.Order == MeshEffectOrder.Above)
                    effect.BuildMesh(meshAsset, order);
            
            builder.GenerateUV(uvGenerator);
        }
        
        void OnDrawGizmosSelected() {
            if (scalingMode == MeshUtils.Scaling.Slice && transform is RectTransform rectTransform) {
                Gizmos.color = new Color(1f, 1f, 0f, 0.25f);
                
                Rect rect = rectTransform.rect;

                float s = Mathf.Min(1,
                    rect.width / borders.Horizontal,
                    rect.height / borders.Vertical);

                Gizmos.DrawLine(
                    transform.TransformPoint(new Vector2(rect.xMin + borders.Left * s, rect.yMin)),
                    transform.TransformPoint(new Vector2(rect.xMin + borders.Left * s, rect.yMax)));
                
                Gizmos.DrawLine(
                    transform.TransformPoint(new Vector2(rect.xMax - borders.Right * s, rect.yMin)),
                    transform.TransformPoint(new Vector2(rect.xMax - borders.Right * s, rect.yMax)));
                
                Gizmos.DrawLine(
                    transform.TransformPoint(new Vector2(rect.xMin, rect.yMin + borders.Bottom * s)),
                    transform.TransformPoint(new Vector2(rect.xMax, rect.yMin + borders.Bottom * s)));
                
                Gizmos.DrawLine(
                    transform.TransformPoint(new Vector2(rect.xMin, rect.yMax - borders.Top * s)),
                    transform.TransformPoint(new Vector2(rect.xMax, rect.yMax - borders.Top * s)));
            }
        }

        #region IMeshEffectTarget

        List<IMeshEffect> effects = new List<IMeshEffect>();

        public void AddEffect(IMeshEffect meshEffect) {
            if (meshEffect != null && !effects.Contains(meshEffect))
                effects.Add(meshEffect);
        }

        public void RemoveEffect(IMeshEffect meshEffect) {
            if (meshEffect != null && effects.Contains(meshEffect)) {
                effects.Remove(meshEffect);
                SetDirty();   
            }
        }

        #endregion
    }
}
