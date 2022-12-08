using System;
using System.Collections;
using UnityEngine;
using Yurowm;
using Yurowm.ContentManager;
using Yurowm.Extensions;

namespace YMatchThree.Core {
    [RequireComponent(typeof(SpriteRenderer))]
    public class ContentOverlay : BaseBehaviour {
        
        public float alpha = .5f;
        
        Sprite sprite;
        
        public void SetOverlay(Sprite sprite) {
            this.sprite = sprite;
            if (spriteChanger == null)
                spriteChanger = SpriteChanger();
        }

        void OnEnable() {
            spriteChanger = null;
            spriteRenderer.sprite = sprite;
            spriteRenderer.color = spriteRenderer.color.Transparent(sprite ? alpha : 0);
        }

        void OnDisable() {
            spriteChanger = null;
            sprite = null;
            spriteRenderer.color = spriteRenderer.color.Transparent(0);
            spriteRenderer.sprite = sprite;
        }

        IEnumerator spriteChanger;
        IEnumerator SpriteChanger() {
            if (spriteRenderer.color.a == 0 || spriteRenderer.sprite == null) {
                spriteRenderer.color = spriteRenderer.color.Transparent(0);
                spriteRenderer.sprite = null;
            }
            
            while (spriteRenderer.sprite != sprite) {
                while (spriteRenderer.color.a != 0 && spriteRenderer.sprite != sprite) {
                    var c = spriteRenderer.color;
                    c.a = c.a.MoveTowards(0, Time.unscaledDeltaTime);
                    spriteRenderer.color = c;
                    yield return null;
                }   
                
                spriteRenderer.sprite = sprite;
                
                if (sprite)
                    while (spriteRenderer.color.a != alpha && spriteRenderer.sprite == sprite) {
                        var c = spriteRenderer.color;
                        c.a = c.a.MoveTowards(alpha, Time.unscaledDeltaTime);
                        spriteRenderer.color = c;
                        yield return null;
                    }
            }

            spriteChanger = null;
        }
        
        void Update() {
            spriteChanger?.MoveNext();
        }
    }
    
    public interface IContentWithOverlay : ILiveContexted {
        void SetOverlay(Sprite sprite);
    }
}