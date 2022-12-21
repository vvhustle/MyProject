using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace GM
{
    public class TooltipButton : MonoBehaviour
    {
        public bool boldName;
        public string nameKey;
        public string descKey;

        private void Start()
        {
            var btn = GetComponent<Button>();
            btn.onClick.AddListener(() =>
            {
                var sb = new StringBuilder();
                if (!string.IsNullOrEmpty(nameKey))
                {
                    var str = L.Get(nameKey);
                    if (boldName)
                    {
                        sb.Append($"<b>{str}</b>");
                    }
                    else
                    {
                        sb.Append(str);
                    }
                }
                if (!string.IsNullOrEmpty(descKey))
                {
                    var str = L.Get(descKey);
                    sb.Append($"\n{str}");
                }
                Tooltip.ShowText(sb.ToString());
            });
        }
    }
}
