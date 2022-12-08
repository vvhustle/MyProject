using UnityEngine;
using Yurowm.Extensions;

namespace Yurowm.Colors {
    [RequireComponent(typeof (SpriteRenderer))]
    public class SpriteColorRepaint : RepaintColor {
        
        SpriteRenderer spriteRenderer;

        public override void SetColor(Color color) {
            if (!spriteRenderer && !this.SetupComponent(out spriteRenderer))
                return;
            
            spriteRenderer.color = TransformColor(color);
        }

        public override Color GetColor() {
            if (!spriteRenderer && !this.SetupComponent(out spriteRenderer))
                return default;
            
            return spriteRenderer.color;
        }
    }
}