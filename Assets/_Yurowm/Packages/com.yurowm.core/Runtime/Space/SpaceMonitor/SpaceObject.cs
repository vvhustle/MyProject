using System.Linq;
using Yurowm.ContentManager;
using Yurowm.Extensions;

namespace Yurowm.Spaces {
    public class SpaceObject : ContextedBehaviour {

        public SpacePhysicalItemBase item;
    
        public virtual void Destroying() {
            Kill();
        }
    
        #region IColorBlindAgent

        const string colorBlindTag = "ColorBlind";

        public void SetColorBlind(bool enabled) {
            transform
                .AllChild()
                .Where(t => t.CompareTag(colorBlindTag))
                .ForEach(t => t.gameObject.SetActive(enabled));
        }

        #endregion
    }
}