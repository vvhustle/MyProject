using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Yurowm.Extensions;
using Yurowm.Localizations;
using Yurowm.UI;
using Yurowm.Utilities;

namespace Yurowm.ComposedPages {
    public class AcceptPage : Page {
        string text;
        string buttonText;
        Action toAccept;

        public AcceptPage(string text, string buttonText, Action toAccept) {
            this.text = text;
            this.buttonText = buttonText;
            this.toAccept = toAccept;
        }

        public override void Building() {
            var message = AddElement<ComposedText>();
            message.SetText(text);
            message.SetHeight(new FloatRange(100, 400));
            
            var button = AddElement<ComposedButton>();
            button.SetLabel(buttonText);
            message.SetAlignment(TextAlignmentOptions.Center);
            button.button.SetAction(() => {               
                isActual = false;
                Close();
                toAccept();
            });
        }
    }
    
    public class ErrorPage : Page {
        string text;

        public ErrorPage(string text) {
            this.text = text;
        }

        public override void Building() {
            var title = AddElement<ComposedTitle>();
            title.SetTitle(Localization.content[titleKey]);
            title.closeButton = false;
            
            var message = AddElement<ComposedText>();
            message.SetText(text);
            message.SetAlignment(TextAlignmentOptions.Center);
            message.SetHeight(new FloatRange(100, 400));
        }
        
        const string titleKey = "Popups/Error/title";
        
        [LocalizationKeysProvider]
        static IEnumerator GetKeys() {
            yield return titleKey;
        }
    }    
    
    public class InfoPage : Page {
        string title;
        string text;
        Sprite sprite;

        public InfoPage(string title, string text, Sprite sprite = null) {
            this.title = title;
            this.text = text;
            this.sprite = sprite;
            backgroundClose = true;
            interactable = false;
        }

        public override void Building() {
            if (!this.title.IsNullOrEmpty()) {
                var title = AddElement<ComposedTitle>();
                title.SetTitle(this.title);
                title.closeButton = false;
            }
            
            if (sprite) {
                var image = AddElement<ComposedImage>();
                image.SetSprite(sprite);
                image.image.preserveAspect = true;
                image.SetHeight(40);
            }

            var message = AddElement<ComposedText>();
            message.SetText(text.Trim());
            message.SetAlignment(TextAlignmentOptions.Center);
        }
    }

    public class RenamePage : Page {
        Action<string> apply;
        string originalName;
        Func<string, bool> validate;
        
        const string enterKey = "Popups/Rename/enter";

        public RenamePage(string originalName, Action<string> apply, Func<string, bool> validate = null) : base() {
            this.apply = apply;
            this.originalName = originalName;
            if (validate == null)
                validate = name => true;
            this.validate = validate;
        }

        public override void Building() {
            var title = AddElement<ComposedTitle>();
            title.closeButton = true;
            title.SetTitle("Rename");

            string newName = originalName;
            var input = AddElement<ComposedInputField>();
            input.SetTitle("Type a new name:");
            input.SetText(newName);

            var enterButton = AddElement<ComposedButton>();
            enterButton.SetLabel(Localization.content[enterKey]);
            enterButton.onClick += () => {
                apply(newName);
                Close();
                UIRefresh.Invoke();
            };
            
            input.onValueChanged += value => {
                newName = value;
                enterButton.SetInteractable(validate(value));
            };
        }
    }
    
    public class WaitingPage : Page {
        public override void Building() {
            var title = AddElement<ComposedTitle>();
            title.SetTitle("Wait a moment");
            title.closeButton = false;
        }
    }
}