using UnityEngine;
using Yurowm.Extensions;

namespace Yurowm.UI {
    public class ShowPageButton : MonoBehaviour {
        
        public enum Type {
            ByName = 0,
            Previous = 1,
            Default = 2
        }
        
        public Type type;
        [HideInInspector]
        public string pageID;

        void OnEnable() {
            if (this.SetupComponent(out Button button))
                button.SetAction(Show);
        }

        public void Show() {
            switch (type) {
                case Type.Default: Page.GetDefault().Show(); break;
                case Type.Previous: Page.Back(); break;
                case Type.ByName: Page.Get(pageID).Show(); break;
            }
        }
    }
}