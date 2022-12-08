using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Yurowm.ComposedPages;
using Yurowm.ContentManager;
using Yurowm.Coroutines;
using Yurowm.Extensions;
using Yurowm.Integrations;
using Yurowm.Localizations;
using Yurowm.Utilities;

namespace Yurowm.Features {
    public class NoLifePopup : Page {
        const string titleKey = "Popups/NoLife/title";
        const string messageKey = "Popups/NoLife/message";
        const string closeKey = "Popups/NoLife/close";
        
        public override void Building() {
            var lifeSystem = Integration.Get<LifeSystem>();
            
            var title = AddElement<ComposedTitle>();
            title.SetTitle(Localization.content[titleKey]);
            
            if (lifeSystem != null) {
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
                
                var timer = AddElement<ComposedText>();
                timer.SetAlignment(TextAlignmentOptions.Center);
                timer.SetHeight(new FloatRange(30, 60));
                TimerLogic(timer).Run();
            }
            
            var close = AddElement<ComposedButton>();
            close.SetLabel(Localization.content[closeKey]);
            close.onClick = Close;
        }

        IEnumerator TimerLogic(ComposedText timer) {
            var lifeSystem = Integration.Get<LifeSystem>();
            
            while (timer.enabled) {
                
                timer.SetText(LifeSystem.TimeSpanToString(lifeSystem.GetRecoveringTime()).Monospace().Scale(2f));
                
                yield return new WaitTimeSpan(1f);
            }
        }

        [LocalizationKeysProvider]
        static IEnumerator GetKeys() {
            yield return titleKey;
            yield return messageKey;
            yield return closeKey;
        }
    }
}