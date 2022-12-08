using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using YMatchThree;
using YMatchThree.Core;
using YMatchThree.UI;
using Yurowm;
using Yurowm.Advertising;
using Yurowm.ComposedPages;
using Yurowm.Core;
using Yurowm.Coroutines;
using Yurowm.DebugTools;
using Yurowm.Extensions;
using Yurowm.Integrations;
using Yurowm.Localizations;
using Yurowm.Serialization;
using Yurowm.UI;
using Yurowm.Utilities;
using Yurowm.YPlanets;
using Page = Yurowm.ComposedPages.Page;
using Yurowm.Services;

namespace YMatchThree {
    public class SettingsPage : Page, IUIRefresh {
        const string titleKey = "Pages/Settings/title";
        const string languageKey = "Pages/Settings/language";
        const string languagesKey = "Languages/{0}";
        const string languageValueFormat = "{0}<br><size=60%>{1}</size>";
        const string musicKey = "Pages/Settings/music";
        const string soundsKey = "Pages/Settings/sounds";
        const string vibeKey = "Pages/Settings/vibe";
        const string closeKey = "Pages/Settings/close";
        const string restartKey = "Pages/Settings/restart";
        const string restoreKey = "Pages/Settings/restore";
        const string disableADsKey = "Pages/Settings/disableADs";
        const string supportKey = "Pages/Settings/support";
        const string privacyPolicyKey = "Pages/Settings/privacyPolicy";
        
        const string signOutKey = "Pages/Settings/signOut";
        const string saveProgressKey = "Pages/Settings/saveProgress";

        public override void Building() {
            var settings = GameSettings.Instance;
            var projectSettings = PropertyStorage.Load<ProjectSettings>();
            
            var title = AddElement<ComposedTitle>();
            title.SetTitle(Localization.content[titleKey]);
            
            var scroll = AddElement<ComposedContainer>("ComposedScroll"); 
            scroll.layout.flexibleHeight = 1;
            
            #region Language
            
            var languageSelector = scroll.AddElement<ComposedValueSelector>();
            languageSelector.SetTitle(Localization.content[languageKey]);

            LanguageContent.GetSupportedLanguagesProgess(languages => {
                int currentLangID = languages.IndexOf(Localization.language);
                var langNames = languages.Select(s => languageValueFormat.FormatText(Localization.content[languagesKey.FormatText(s)], s));

                languageSelector.SetValues(langNames);
                languageSelector.SetValue(currentLangID);

                languageSelector.onSelect += id => {
                    currentLangID = id;
                    Localization.LearnLanguage(languages[id]).Run();
                };
            }).Run();
            
            #endregion

            #region Music & SFX
            
            var audio = settings.GetModule<Yurowm.Audio.AudioSettings>();
            var vibrator = settings.GetModule<VibratorSettings>();

            var musicSlider = scroll.AddElement<ComposedSlider>();
            musicSlider.title.text = Localization.content[musicKey];
            musicSlider.SetRange(new FloatRange(0f, 1f), audio.MusicVolume);
            musicSlider.onValueChanged = v => audio.MusicVolume = v;

            var soundSlider = scroll.AddElement<ComposedSlider>();
            soundSlider.title.text = Localization.content[soundsKey];
            soundSlider.SetRange(new FloatRange(0f, 1f), audio.SoundVolume);
            soundSlider.onValueChanged = v => audio.SoundVolume = v;
            
            if (Vibrator.HasVibrator()) {
                var vibeToggle = scroll.AddElement<ComposedToggle>();
                vibeToggle.title.text = Localization.content[vibeKey];
                vibeToggle.SetValue(vibrator.vibeEnabled);
                vibeToggle.onValueChanged = vibrator.SetVibeEnable;
            }
            
            #endregion

            #region Session

            var puzzleSpace = Yurowm.Spaces.Space
                .all.CastOne<PuzzleSpace>();    
            
            if (puzzleSpace != null && puzzleSpace.IsActual()) {
                
                var restartButton = scroll.AddElement<ComposedButton>();
                restartButton.SetLabel(Localization.content[restartKey]);
                restartButton.onClick = () => {
                    Close();
                    PuzzleSpace.Restart();
                };
                
                var closeButton = scroll.AddElement<ComposedButton>();
                closeButton.SetLabel(Localization.content[closeKey]);
                closeButton.onClick = () => {
                    Close();
                    PuzzleSpace.Close();
                };
            }

            #endregion
            
            #if PLAYFAB
            var playfabIntegration = Integration.Get<PlayfabIntegration>();
            
            if (playfabIntegration != null && playfabIntegration.active) {
                if (playfabIntegration.IsLoggedIn()) {
                    ComposedButton signOut = scroll.AddElement<ComposedButton>();
                    signOut.SetLabel(Localization.content[signOutKey]);
                    signOut.onClick = playfabIntegration.SignOut;
                } else {
                    ComposedButton save = scroll.AddElement<ComposedButton>();
                    save.SetLabel(Localization.content[saveProgressKey]);
                    save.onClick = PlayfabSignInPopup.Show;
                }
            } 
            #endif
            
            if (!projectSettings.supportEmail.IsNullOrEmpty()) {
                var support = scroll.AddElement<ComposedButton>();
                support.SetLabel(Localization.content[supportKey]);
                support.onClick = App.OpenHelpPopup;
            }
            
            if (Application.platform == RuntimePlatform.IPhonePlayer) {
                var gameCenter = scroll.AddElement<ComposedButton>();
                gameCenter.SetLabel("Game Center");
                gameCenter.onClick = () => {
                    #if UNITY_IOS
                    UnityEngine.SocialPlatforms.GameCenter.GameCenterPlatform.ShowLeaderboardUI("bestScore", TimeScope.AllTime);
                    #endif
                };
            }

            #if UNITY_IAP
            
            if (GameSettings.Instance.GetModule<AdvertsSettings>().adsEnabled) {
                ComposedButton disableADs = scroll.AddElement<ComposedButton>();
                disableADs.SetLabel(Localization.content[disableADsKey]);
                disableADs.onClick = DisableADs;
            }
            
            var purchasing = Integration.storage.items.CastOne<UnityIAPIntegration>();
            if (Application.platform == RuntimePlatform.IPhonePlayer) {
                ComposedButton restore = scroll.AddElement<ComposedButton>();
                restore.SetLabel(Localization.content[restoreKey]);
                restore.onClick = purchasing.RestorePurchases;
            }
            
            #endif
            
            var privacyPolicyURL = GameParameters.GetModule<GameParametersGeneral>().privacyPolicyURL;
            if (!privacyPolicyURL.IsNullOrEmpty()) {
                var privacyPolicy = scroll.AddElement<ComposedButton>();
                privacyPolicy.SetLabel(Localization.content[privacyPolicyKey]);
                privacyPolicy.onClick = () => Application.OpenURL(privacyPolicyURL);
            }
        }

        #if UNITY_IAP
        
        void DisableADs() {
            var integration = Integration.storage.items.CastOne<UnityIAPIntegration>();
            var iap = integration?.GetIAP("disableads");
                
            if (iap == null) return;
                
            integration.Purchase(iap);
        }
        
        #endif

        [LocalizationKeysProvider]
        static IEnumerator GetKeys() {
            yield return titleKey;
            yield return languageKey;
            
            yield return LanguageContent
                .GetSupportedLanguages()
                .Select(l => languagesKey.FormatText(l));
            
            yield return musicKey;
            yield return soundsKey;
            yield return vibeKey;
            yield return closeKey;
            yield return restartKey;
            yield return restoreKey;
            yield return disableADsKey;
            yield return privacyPolicyKey;
            yield return supportKey;
            
            yield return signOutKey;
            yield return saveProgressKey;
        }
    }
}