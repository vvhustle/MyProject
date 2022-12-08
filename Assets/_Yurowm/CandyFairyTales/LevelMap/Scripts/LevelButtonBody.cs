using UnityEngine;
using Yurowm;
using Yurowm.ContentManager;
using Yurowm.Extensions;
using Yurowm.Spaces;
using Yurowm.UI;

namespace YMatchThree.Seasons {
    public class LevelButtonBody : SpaceObject, IReserved {
        LabelFormat[] labels;
        public Transform[] stars;

        void Awake() {
            labels = GetComponentsInChildren<LabelFormat>(true);
        }

        public void Rollout() {
            SetText(string.Empty);
        }
        
        public void SetText(string text) {
            labels.ForEach(l => l["number"] = text);
        }
        
        public void SetStars(int count) {
            count = count.Clamp(0, 3);
            
            for (var i = 0; i < stars.Length; i++) 
                stars[i].gameObject.SetActive(i < count);
        }
    }
}