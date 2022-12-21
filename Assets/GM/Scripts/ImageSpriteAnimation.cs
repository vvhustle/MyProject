using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace GM
{
    public class ImageSpriteAnimation : MonoBehaviour
    {
        public Sprite[] sprites;
        public float fps = 1.0f;
        public float delay = 0.0f;
        public int loops = 1;
        public bool hideOnEnd = false;

        private IEnumerator Start()
        {
            var img = GetComponent<Image>();
            yield return new WaitForSeconds(delay);
            var wait = new WaitForSeconds(fps);
            for (int l = 0; l < loops; ++l)
            {
                for (int i = 0; i < sprites.Length; ++i)
                {
                    img.sprite = sprites[i];
                    yield return wait;
                }
            }
            if (hideOnEnd)
            {
                img.enabled = false;
            }
        }
    }
}
