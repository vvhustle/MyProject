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
    public class PlayfabSignInPopup : Page {
        
        string email = "";
        string password = "";
        
        bool processing = false;
        
        
        ComposedText message;
        
        public static void Show() {
            ComposedPage.ByID("FullScreen")?.Show(new PlayfabSignInPopup());
        }
        
        public override void Building() {
            if (processing) {
                AddElementWithSuffix<ComposedOther>("LoadingLoop");
                return;
            }

            var title = AddElement<ComposedTitle>();
            title.SetTitle("Sign In");
            title.closeButton = true;
            
            var scroll = AddElement<ComposedContainer>("ComposedScroll");
            scroll.layout.flexibleHeight = 1;
            
            var emailInput = scroll.AddElementWithSuffix<ComposedInputField>("Vertical");
            emailInput.SetTitle("Email");
            emailInput.SetText(email);
            emailInput.onValueChanged = v => email = v;
            
            var passwordInput = scroll.AddElementWithSuffix<ComposedInputField>("Vertical");
            passwordInput.SetTitle("Password");
            passwordInput.SetText(password);
            passwordInput.SetInputType(TMP_InputField.InputType.Password);
            passwordInput.onValueChanged = v => password = v;
            
            message = scroll.AddElement<ComposedText>();
            message.SetAlignment(TextAlignmentOptions.Center);
            message.SetHeight(new FloatRange(30, 130));
            message.SetText("");
            
            #if PLAYFAB
            
            var playfabIntegration = Integration.Get<PlayfabIntegration>();

            #if SIGN_IN_WITH_APPLE
            
            var signInWithAppleIntegration = Integration.Get<SignInWithAppleIntegration>();
            
            if (signInWithAppleIntegration != null && signInWithAppleIntegration.IsSupportedPlatform()) {
                var signApple = AddElementWithSuffix<ComposedButton>("SignInApple");
                signApple.SetLabel("Sign in with Apple");
                signApple.onClick += () => {
                    processing = true;
                    Refresh();
                    
                    playfabIntegration.SignInApple(
                        onSuccess: Close,
                        onFailed: error => {
                            processing = false;
                            Refresh();
                            if (!error.IsNullOrEmpty())
                                message.SetText(error);
                        });
                };
            }
            
            #endif
            
            #if FACEBOOK
            
            var facebookIntegration = Integration.Get<FacebookIntegration>();
            
            if (facebookIntegration != null) {
                var signFacebook = AddElementWithSuffix<ComposedButton>("SignInFacebook");
                signFacebook.SetLabel("Sign in with Facebook");
                signFacebook.onClick += () => {
                    processing = true;
                    Refresh();
                        
                    playfabIntegration.SignInFacebook(
                        onSuccess: Close,
                        onFailed: error => {
                            processing = false;
                            Refresh();
                            if (!error.IsNullOrEmpty())
                                message.SetText(error);
                        });
                };
            }
            
            #endif
            #endif
            
            var signUpButton = AddElement<ComposedButton>();
            signUpButton.SetLabel("Sign Up");
            signUpButton.onClick += () => 
                PlayfabSignUpEmailPopup.Show();
            
            var enterButton = AddElement<ComposedButton>();
            enterButton.SetLabel("Submit");
            enterButton.onClick += () => {
                if (!Validate()) {
                    return;
                }
                
                processing = true;
                
                Refresh();

                playfabIntegration.SignIn(email, password, 
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
        
        static readonly Regex emailValidation = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
        static readonly Regex passwordValidation = new Regex(@"^.{1,32}$");
        
        bool Validate() {
            if (email.IsNullOrEmpty()) {
                message.SetText("Write your email");
                return false;
            }
            
            if (password.IsNullOrEmpty()) {
                message.SetText("Write your password");
                return false;
            }
            
            if (!emailValidation.IsMatch(email)) {
                message.SetText("Wrong email format");
                return false;
            }
            
            if (!passwordValidation.IsMatch(password)) {
                message.SetText("Password is too long. It must to contain 32 symbols maximum.");
                return false;
            }
            
            message.SetText(string.Empty);
            
            return true;
        }

        #endregion
    }
}
#endif