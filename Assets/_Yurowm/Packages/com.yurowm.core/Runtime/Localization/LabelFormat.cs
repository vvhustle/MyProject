using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Yurowm.Localizations;
using System.Collections;
using Yurowm.Extensions;

namespace Yurowm.UI {
    public class LabelFormat : LabelTextProviderBehaviour, ILocalizedComponent {

        Text label;
        TMP_Text tmpLabel;

        [HideInInspector]
        public bool localized = false;
        [HideInInspector]
        public string format = "";
        [HideInInspector]
        public string localizationKey = "";

        public string Format {
            get {
                if (localized)
                    return Localization.content?[localizationKey] ?? localizationKey;
                return format;
            }
        }

        Dictionary<string, string> dictionary = new Dictionary<string, string>();

        public string this[string index] {
            get => dictionary.Get(index);
            set {
                dictionary[index] = value;
                SetDirty();
            }
        }

        public override void Initialize() {
            base.Initialize();
            InitializeWords();
        }

        static readonly Regex wordFormat = new Regex(@"\{(?<word>[\@A-Za-z0-9_]+)\}");
        void InitializeWords() {
            foreach (Match match in wordFormat.Matches(Format)) {
                string key = match.Groups["word"].Value;
                if (!dictionary.ContainsKey(key))
                    dictionary.Add(key, key);
            }
        }

        protected override void OnEnable() {
            InitializeWords();
            base.OnEnable();
        }

        public override string GetText() {
            string result = Format;
            foreach (var word in dictionary) {
                string value = word.Key.StartsWith("@") ? 
                    ReferenceValues.Get(word.Key.Substring(1)).ToString() : word.Value;
                result = result.Replace("{" + word.Key + "}", value);
            }
            return result;
        }

        #region ILocalizedComponent
        public IEnumerable GetLocalizationKeys() {
            if (localized)
                yield return localizationKey;
        }
        #endregion
    }
}