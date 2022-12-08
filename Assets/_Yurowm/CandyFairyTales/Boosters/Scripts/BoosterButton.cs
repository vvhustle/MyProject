using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Yurowm.Extensions;
using Yurowm.UI;
using Button = Yurowm.Button;

namespace YMatchThree.Core {
    public class BoosterButton : VirtualizedScrollItemBody {
        public Booster booster;
        
        public Image icon;
        
        public TMP_Text count;
        public GameObject empty;
        public GameObject selected;

        public TMP_Text title;
        public TMP_Text description;
        
        public override void Initialize() {
            base.Initialize();
            if (this.SetupComponent(out Button button))
                button.onClick.SetSingleListner(OnClick);
        }

        void OnClick() {
            booster.OnClick();
        }
    }
}