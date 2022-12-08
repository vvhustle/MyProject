using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Yurowm.ComposedPages;
using Yurowm.Extensions;
using Yurowm.Localizations;
using Yurowm.Serialization;
using Yurowm.Utilities;

namespace YMatchThree.Meta {
    public class PlayerDataConflictPopup : Page {
        
        GameData.Module[] cloud;
        GameData.Module[] local;
        
        public PlayerDataConflictPopup(IEnumerable<GameData.Module> cloud, IEnumerable<GameData.Module> local) {
            this.cloud = cloud.ToArray();
            this.local = local.ToArray();
        }

        public void Show() {
            ComposedPage.ByID("FullScreen").Show(this);
        }
        
        string GetDescription(GameData.Module[] data) {
            return data.CastOne<PlayerData.DataInfo>().lastSaveTime.ToString();
        }
        
        public override void Building() {
            var title = AddElement<ComposedTitle>();
            title.SetTitle(titleKey.localized());
            title.closeButton = false;
            
            var description = AddElement<ComposedText>();
            description.SetAlignment(TextAlignmentOptions.Center);
            description.SetHeight(new FloatRange(30, 250));
            description.SetText(titleKey.localized());
            
            AddElement<ComposedFlexibleSpace>();
            
            var cloudDescription = AddElement<ComposedText>();
            cloudDescription.SetAlignment(TextAlignmentOptions.Center);
            cloudDescription.SetHeight(new FloatRange(30, 250));
            cloudDescription.SetText(GetDescription(cloud));
            
            var useCloud = AddElement<ComposedButton>();
            useCloud.SetLabel(cloudKey.localized());
            useCloud.onClick += () => Use(cloud);
            
            var localDescription = AddElement<ComposedText>();
            localDescription.SetAlignment(TextAlignmentOptions.Center);
            localDescription.SetHeight(new FloatRange(30, 250));
            localDescription.SetText(GetDescription(local));

            var useLocal = AddElement<ComposedButton>();
            useLocal.SetLabel(localKey.localized());
            useLocal.onClick += () => Use(local);
        }
        
        void Use(GameData.Module[] data) {
            
            PlayerData.SetRawData(PlayerData.ModulesToRawData(data), true);

            PlayerData.TryToSave();
            
            Close();
        }
        
        const string titleKey = "Popups/PlayerConflict/title";
        const string descriptionKey = "Popups/PlayerConflict/description";
        const string cloudKey = "Popups/PlayerConflict/cloud";
        const string localKey = "Popups/PlayerConflict/local";
        
        [LocalizationKeysProvider]
        static IEnumerator GetKeys() {
            yield return titleKey;
            yield return descriptionKey;
            yield return cloudKey;
            yield return localKey;
        }

    }
}