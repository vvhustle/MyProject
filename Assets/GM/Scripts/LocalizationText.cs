using UnityEngine;
using UnityEngine.UI;

namespace GM
{
    public class LocalizationText : MonoBehaviour
    {
        public string key;

        private Text _label;

        void Start()
        {
            _label = GetComponent<Text>();
            ChangeLocalization();
            L.LanguageChanged.AddListener(ChangeLocalization);
        }

        public void ChangeLocalization()
        {
            if (!_label || string.IsNullOrEmpty(key))
                return;

            var text = L.Get(key);
            if (!string.IsNullOrEmpty(text))
            {
                _label.text = text;
            }
#if UNITY_EDITOR
            else
            {
                _label.text = $"!{key}";
            }
#endif
        }
    }
}