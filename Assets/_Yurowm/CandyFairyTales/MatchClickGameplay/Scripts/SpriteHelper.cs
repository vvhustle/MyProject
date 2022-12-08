using UnityEngine;

namespace YMatchThree.Core {
    public class SpriteHelper : MonoBehaviour {
        new SpriteRenderer renderer;

        void Awake () {
            renderer = GetComponent<SpriteRenderer>();
        }

        public void SetTexture(Texture2D texture) {
            renderer.material.SetTexture("_Tex", texture);
            renderer.material.SetVector("_Size", renderer.bounds.size);        
        }
    }
}