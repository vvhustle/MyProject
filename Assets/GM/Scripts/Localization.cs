using UnityEngine;
using UnityEngine.Events;

namespace GM
{
    public static class L
    {
        public static UnityEvent LanguageChanged = new UnityEvent();

        private static readonly string PrefsKey = "L.CurrentLanguage";
        public static string CurrentLanguage
        {
            get
            {
                var lang = PlayerPrefs.GetString(PrefsKey, "English");
                return lang;
            }
            set
            {
                PlayerPrefs.SetString(PrefsKey, value);
                LanguageChanged.Invoke();
            }
        }

        public static string Get(string key)
        {
            var table = LuaMain.Instance.CurrentState.GetTable("GameData.text")[key] as LuaInterface.LuaTable;
            if (table == null)
            {
                return "";
            }
            return table[CurrentLanguage].ToString();
        }

        public static string Get(string key, params object[] args)
        {
            var get = Get(key);
            if (string.IsNullOrEmpty(get))
                return "";

            return string.Format(get, args);
        }
    }
}
