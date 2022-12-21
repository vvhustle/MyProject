using LuaInterface;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GM
{
    public class LuaButton : MonoBehaviour
    {
        private Dictionary<string, LuaFunction> buttons = new Dictionary<string, LuaFunction>();

        protected void OnDestroy()
        {
            ClearClick();
        }

        public void AddClick(GameObject go, LuaFunction luafunc)
        {
            if (go == null || luafunc == null) return;
            buttons.Add(go.name, luafunc);
            go.GetComponent<Button>().onClick.AddListener(() => {
                luafunc.Call(go);
            });
        }

        public void RemoveClick(GameObject go)
        {
            ClearClick();
        }

        public void ClearClick()
        {
            foreach (var de in buttons)
            {
                if (de.Value != null)
                {
                    de.Value.Dispose();
                }
            }
            buttons.Clear();
            gameObject.GetComponent<Button>().onClick.RemoveAllListeners();
        }
    }
}
