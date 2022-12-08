#if PLAYFAB
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using Yurowm.ComposedPages;
using Yurowm.Extensions;
using Yurowm.Integrations;
using Yurowm.UI;
using Yurowm.Utilities;
using Page = Yurowm.ComposedPages.Page;

namespace Yurowm.Services {
    public class PlayfabSignUpEmailPopup : Page {
        
        string username = "";
        string email = "";
        string password1 = "";
        string password2 = "";
        
        bool processing = false;
        
        PlayfabIntegration integration = Integration.Get<PlayfabIntegration>();
        
        ComposedInputField passwordInput;
        ComposedInputField passwordConfirmInput;
        
        public static void Show() {
            ComposedPage.ByID("FullScreen")?.Show(new PlayfabSignUpEmailPopup());
        }
        
        public override void Building() {
            if (processing) {
                AddElementWithSuffix<ComposedOther>("LoadingLoop");
                return;
            }
            
            var title = AddElement<ComposedTitle>();
            title.SetTitle("Sign Up");
            title.closeButton = true;
            
            var scroll = AddElement<ComposedContainer>("ComposedScroll");
            scroll.layout.flexibleHeight = 1;

            var nameInput = scroll.AddElementWithSuffix<ComposedInputField>("Vertical");
            nameInput.SetTitle("Name");
            nameInput.SetText(username);
            nameInput.onValueChanged = v => username = v;
                
            var emailInput = scroll.AddElementWithSuffix<ComposedInputField>("Vertical");
            emailInput.SetTitle("Email");
            emailInput.SetText(email);
            emailInput.onValueChanged = v => email = v;
            
            passwordInput = scroll.AddElementWithSuffix<ComposedInputField>("Vertical");
            passwordInput.SetTitle("Password");
            passwordInput.SetText(password1);          
            passwordInput.onValueChanged = v => password1 = v;
            passwordInput.SetInputType(TMP_InputField.InputType.Password);
            
            passwordConfirmInput = scroll.AddElementWithSuffix<ComposedInputField>("Vertical");
            passwordConfirmInput.SetTitle("Confirm");
            passwordConfirmInput.SetText(password2);
            passwordConfirmInput.onValueChanged = v => password2 = v;
            passwordConfirmInput.SetInputType(TMP_InputField.InputType.Password);
            
            message = scroll.AddElement<ComposedText>();
            message.SetAlignment(TextAlignmentOptions.Center);
            message.SetHeight(new FloatRange(30, 130));
            message.SetText("");
            
            var enterButton = AddElement<ComposedButton>();
            enterButton.SetLabel("Submit");
            enterButton.onClick += () => {
                if (!Validate()) {
                    return;
                }
                
                processing = true;
                Refresh();

                integration.SignUp(email, username, password1, 
                    onSuccess: Close,
                    onFailed: error => {
                        processing = false;
                        Refresh();
                        if (!error.IsNullOrEmpty())
                            message.SetText(error);
                    });
            };
        }

        #region Validation
        
        ComposedText message;
        
        static readonly Regex nameValidation = new Regex(@"^([\w\.\-]{2,16})$");
        static readonly Regex emailValidation = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
        static readonly Regex passwordValidation = new Regex(@"^.{6,32}$");
        
        bool Validate() {
            if (!nameValidation.IsMatch(username)) {
                message.SetText("Wrong name format. It must be word of 2-16 symbols.");
                return false;
            }
        
            if (!emailValidation.IsMatch(email)) {
                message.SetText("Wrong email format");
                return false;
            }
            
            if (!passwordValidation.IsMatch(password1)) {
                message.SetText("Too short or too long password. It must contain 6 - 32 symbols.");
                return false;
            }
            
            if (password1 != password2) {
                message.SetText("Password confirmition is failed. Please, type again.");
                password1 = "";
                password2 = "";
                passwordInput.SetText(password1);
                passwordConfirmInput.SetText(password2);
                return false;
            }
            
            message.SetText("");
            
            return true;
        }

        #endregion
    }
}
#endif