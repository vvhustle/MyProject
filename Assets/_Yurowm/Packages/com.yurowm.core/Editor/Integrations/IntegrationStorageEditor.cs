using System.Linq;
using UnityEngine;
using Yurowm.Dashboard;
using Yurowm.Icons;
using Yurowm.Serialization;

namespace Yurowm.Integrations {
    [DashboardGroup("Content")]
    [DashboardTab("Integrations", "Integrations", "tab.integrations")]
    public class IntegrationStorageEditor : StorageEditor<Integration> {
        
        static Texture2D statusIcon;
        
        static readonly Color activeColor = new Color(.5f, 1f, .5f);
        static readonly Color problemColor = new Color(1f, 0.99f, 0.48f);
        static readonly Color inactiveColor = new Color(0.53f, 0.03f, 0.07f, 0.5f);

        int noSDKTag;
        
        public override bool Initialize() {
            noSDKTag = tags.New("NoSDK");
            return base.Initialize();
        }

        public override void OnGUI() {
            if (statusIcon == null) statusIcon = EditorIcons.GetIcon("Dot");
            base.OnGUI();
        }
        
        protected override bool FilterNewItem(Integration item, out string reason) {
            if (!base.FilterNewItem(item, out reason))
                return false;
            
            if (storage.items.Any(i => i.GetType() == item.GetType())) {
                reason = "The storage already contains an integration of this type";
                return false;
            }
            
            return true;
        }

        protected override void Sort() {}

        protected override Rect DrawItem(Rect rect, Integration item) {
            Color color;
            
            if (!item.active)
                color = inactiveColor;
            else if (!item.HasAllNecessarySDK())
                color = problemColor;
            else
                color = activeColor;
            
            rect = ItemIconDrawer.Draw(rect, statusIcon, color);
            return base.DrawItem(rect, item);
        }

        protected override void UpdateTags(Integration item) {
            base.UpdateTags(item);
            tags.Set(item, noSDKTag, !item.HasAllNecessarySDK());
        }

        public override string GetItemName(Integration item) {
            return item.GetName();
        }

        public override Storage<Integration> OpenStorage() {
            return Integration.storage;
        }
    }

}