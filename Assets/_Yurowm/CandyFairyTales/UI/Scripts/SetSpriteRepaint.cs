using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Yurowm.Colors;
using Yurowm.Extensions;

namespace YMatchThree.Core {
    public class SetSpriteRepaint : Repaint {
        
        public List<SpriteInfo> sprites = new List<SpriteInfo>();
        
        public bool setSpriteMask = false;
        
        [Serializable]
        public class SpriteInfo {
            public int colorID;
            public Sprite sprite;
        }
        
        public Sprite GetSprite(int colorID) {
            return sprites.FirstOrDefault(x => x.colorID == colorID)?.sprite;
        }

        Action<Sprite> SetSprite;

        void Awake() {
            Initialize();
        }

        void Initialize() {
            if (this.SetupComponent(out SpriteRenderer spriteRenderer)) {
                SetSprite = s => spriteRenderer.sprite = s;
                
                if (setSpriteMask && this.SetupComponent(out SpriteMask spriteMask))
                    SetSprite += s => spriteMask.sprite = s;
                
                return;
            }
            if (this.SetupComponent(out Image image)) {
                SetSprite = s => image.sprite = s;
                return;
            }
        }

        public static void Repaint(GameObject gameObject, int colorID) {
            foreach (var component in gameObject.GetComponentsInChildren<SetSpriteRepaint>(true)) 
                component.SetColor(colorID);
        }
        
        void SetColor(int colorID) {
            SetSprite?.Invoke(GetSprite(colorID));
        }
    }
}