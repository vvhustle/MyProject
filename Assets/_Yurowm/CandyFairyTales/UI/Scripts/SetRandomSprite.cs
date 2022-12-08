using UnityEngine;
using Yurowm.Extensions;
using Yurowm.Utilities;

namespace YMatchThree.Core {
    public class SetRandomSprite : MonoBehaviour {
        new SpriteRenderer renderer;
        
        public Sprite[] sprites;


        void OnEnable() {
            if (!renderer && !this.SetupComponent(out renderer))
                return;
            
            renderer.sprite = sprites.GetRandom();
        }
    }
}