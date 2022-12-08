using System;
using System.Collections;

namespace Yurowm.UI {
    public class VirtualizedScrollLegacy : VirtualizedScrollBase<VirtualizedScrollItem, object> {
        VirtualizedScrollItem originalItem;
        
        public override void Initialize() {
            base.Initialize();
            
            originalItem = GetComponentInChildren<VirtualizedScrollItem>();
            if (!originalItem) return;
            
            childs.Add(-1, originalItem);
            
            originalItem.gameObject.SetActive(false);
            
            SetupItem(originalItem.transform);
            
            UpdateChilds(0);
        }

        public override VirtualizedScrollItem EmitItem() {
            return Instantiate(originalItem);
        }

        public override IList GetList() {
            return originalItem?.SourceProvider();
        }

        public override void UpdateItem(VirtualizedScrollItem item, object info) {
            item.scroll = this;
            item.UpdateInfo(info);
        }

    }
    
    public abstract class VirtualizedScrollItem : Behaviour {
        [NonSerialized]
        public VirtualizedScrollLegacy scroll = null;
        public abstract void UpdateInfo(object info);
        public abstract IList SourceProvider();
    }

    public abstract class VirtualizedScrollItem<T> : VirtualizedScrollItem where T : class {
        public abstract void UpdateInfo(T info);

        public override void UpdateInfo(object info) {
            if (info is T t)
                UpdateInfo(t);
        }
    }
}
