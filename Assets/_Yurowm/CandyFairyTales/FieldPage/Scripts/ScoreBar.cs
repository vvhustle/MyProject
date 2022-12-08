using UnityEngine;
using Yurowm;
using Yurowm.Utilities;
using Behaviour = Yurowm.Behaviour;

namespace YMatchThree.UI {
    public class ScoreBar : Behaviour {
        public RectTransform fillRect;
        
        public float widthMin = 0;
        
        public void SetFillValue(float value) {
            value = Mathf.Clamp01(value);
            
            if (value > 0) {
                float width = fillRect.parent.rect().rect.width;
                
                fillRect.anchorMin = Vector2.zero;
                fillRect.anchorMax = Vector2.one;
                fillRect.offsetMin = Vector2.zero;
                
                fillRect.offsetMax = new Vector2(-Mathf.Lerp(width - Mathf.Max(widthMin, 0), 0, value), 0);
            } 
            
            fillRect.gameObject.SetActive(value > 0);
        }
    }
}