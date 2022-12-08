using System.Collections;
using System.Linq;
using Yurowm.Extensions;
using Yurowm.UI;

namespace Yurowm.Store {
    public class StoreList : VirtualizedScroll {
        public string group;
        
        StoreGood[] goods;

        public override void Initialize() {
            SetList(StoreItem.storage
                .CastIfPossible<StoreGood>()
                .Where(i => group.IsNullOrEmpty() || i.group == group)
                .OrderBy(i => i.order));
            base.Initialize();
        }

        #region Localization

        public const string localizationKeyPath = "Pages/Store/";

        #endregion
    }
}