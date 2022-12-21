using LuaInterface;
using UnityEngine;

namespace GM
{
    public class LuaComponent : MonoBehaviour
    {
        private LuaState luaState;
        public TextAsset textAsset;

        private void Start()
        {
            if (LuaMain.Instance == null)
            {
                new LuaResLoader();
                luaState = new LuaState();
                LuaBinder.Bind(luaState);
                luaState.Start();
            }
            else
            {
                luaState = LuaMain.Instance.CurrentState;
            }
            luaState["_gameObject"] = gameObject;
            luaState["_transform"] = transform;
            luaState.DoString(textAsset.text);
            var destroyTime = luaState["_destroyTime"];
            if (destroyTime != null)
            {
                Destroy(gameObject, System.Convert.ToInt64(destroyTime));
            }
        }
    }
}
