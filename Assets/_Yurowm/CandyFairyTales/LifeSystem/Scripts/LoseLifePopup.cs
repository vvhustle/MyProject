using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Yurowm.ComposedPages;
using Yurowm.ContentManager;
using Yurowm.Localizations;
using Yurowm.Utilities;

namespace Yurowm.Features {
    public class LoseLifePopup : Page {
        const string titleKey = "Popups/LoseLife/title";
        const string messageKey = "Popups/LoseLife/message";
        const string resumeKey = "Popups/LoseLife/resume";
        const string closeKey = "Popups/LoseLife/close";
        public Action accept;

        public override void Building() {
            var title = AddElement<ComposedTitle>();
            title.SetTitle(Localization.content[titleKey]);

            var sprite = AssetManager.GetAsset<Sprite>("UI_BrokenHeart");

            if (sprite) {
                var icon = AddElement<ComposedImage>();
                icon.SetSprite(sprite);
                icon.SetHeight(120);
            }

            var message = AddElement<ComposedText>();
            message.SetText(Localization.content[messageKey]);
            message.SetAlignment(TextAlignmentOptions.Center);
            message.SetHeight(new FloatRange(50, 200));

            var resume = AddElement<ComposedButton>();
            resume.SetLabel(Localization.content[resumeKey]);
            resume.onClick = Close;
            
            var close = AddElement<ComposedButton>();
            close.SetLabel(Localization.content[closeKey]);
            close.onClick = () => {
                accept?.Invoke();
                Close();
            };
        }

        [LocalizationKeysProvider]
        static IEnumerator GetKeys() {
            yield return titleKey;
            yield return messageKey;
            yield return resumeKey;
            yield return closeKey;
        }
    }
}