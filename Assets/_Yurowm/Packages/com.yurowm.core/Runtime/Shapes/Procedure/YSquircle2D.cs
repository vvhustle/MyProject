using UnityEngine;
using Yurowm.Extensions;

namespace Yurowm.Shapes {
    
    [RequireComponent(typeof(RectTransform))]
    public class YSquircle2D : Shape2DBehaviour {
        YSquircle squircle = new YSquircle();
        
        public Color Color {
            get => color;
            set {
                color = value;
                SetDirty();
            }
        }

        public Color32 color = Color.white;
        
        public MeshBuilderBase.MeshOptimization optimizeMesh = 0;
        
        
        [Range(0.05f, 1f)]
        [SerializeField]
        float _Power = .5f;
        public float Power {
            set {
                if (value == _Power) return;
                _Power = value;
                SetDirty();
            }
            get => _Power;
        }
        
        [SerializeField]
        int _Details = 32;
        public int Details {
            set {
                if (value == _Details) return;
                _Details = value;
                SetDirty();
            }
            get => _Details;
        }

        RectTransform rectTransform;
        
        public override void FillMesh(MeshBuilder builder) {
            if (!rectTransform && !this.SetupComponent(out rectTransform))
                return;
            
            var order = new YSquircle.Order {
                power = _Power,
                color = color,
                details = _Details,
                size = rectTransform.rect.size,
                pivot = rectTransform.pivot
            };

            squircle.FillMesh(builder, order);
            
            builder.Optimize(optimizeMesh);
        }

        void OnRectTransformDimensionsChange() {
            SetDirty();
        }
    }
}