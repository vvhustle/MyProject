using Yurowm.Extensions;

namespace Yurowm.UI {
    public class UIOrientation : UIOrientationBase {

        public Orientation orientation;

        public override void OnScreenResize() {
            gameObject.SetActive(GetCurrentOrientation().HasFlag(orientation));
        }
    }
}