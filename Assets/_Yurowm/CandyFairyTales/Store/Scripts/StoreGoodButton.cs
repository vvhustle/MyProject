using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Yurowm.UI;

namespace Yurowm.Store {
    public class StoreGoodButton : VirtualizedScrollItemBody {
        public Image icon;
        public TMP_Text title;
        public TMP_Text details;
        public TMP_Text cost;
        public Button buy;
        public TMP_Text noAction;
        public GameObject savings;
        
        public override void Rollout() {
            base.Rollout();
            if (savings)
                savings.SetActive(false);
        }
    }
}