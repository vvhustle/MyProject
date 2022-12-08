using UnityEngine;
using Yurowm.ContentManager;

namespace Yurowm.UI {
    public class VirtualizedScrollItemBody : ContextedBehaviour, IReserved {
        public Vector2 size = new Vector2(300, 100);

        public virtual void Rollout() {}
    }
}