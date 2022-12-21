using LuaInterface;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace GM
{
    public class LuaMain : MonoBehaviour
    {
        public static LuaMain Instance;

        [SerializeField] private int selectEnv = -1;
        public string[] envUris;
        public bool LogGC = false;
        public bool LogDebugger = false;

        public LuaState CurrentState { get; private set; } = null;

        private void Awake()
        {
            Instance = this;

            new LuaResLoader();

            var luaPath = Path.Combine(Application.persistentDataPath, "lua");
            if (!Directory.Exists(luaPath)) Directory.CreateDirectory(luaPath);
            LuaFileUtils.Instance.AddSearchPath(Path.Combine(Application.persistentDataPath, "lua"));

            Debugger.useLog = LogDebugger;

            CurrentState = new LuaState();
            CurrentState.LogGC = LogGC;
            CurrentState.Start();

            CurrentState.LuaGetField(LuaIndexes.LUA_REGISTRYINDEX, "_LOADED");
            CurrentState.OpenLibs(LuaDLL.luaopen_cjson);
            CurrentState.LuaSetField(-2, "cjson");
            CurrentState.OpenLibs(LuaDLL.luaopen_cjson_safe);
            CurrentState.LuaSetField(-2, "cjson.safe");
            CurrentState.LuaSetTop(0);

            LuaBinder.Bind(CurrentState);
            DelegateFactory.Init();

            CurrentState["OS"] = SystemInfo.operatingSystem.Substring(0, SystemInfo.operatingSystem.IndexOf(' '));
        }

        private IEnumerator Start()
        {
            int env = PlayerPrefs.GetInt("env", -1);
            if (env >= 0)
            {
                selectEnv = env;
            }
            CurrentState["ServerUri"] = envUris[selectEnv];
            if (selectEnv >= 0)
            {
                var mainUri = $"{envUris[selectEnv]}/lua/main.lua";
                var request = UnityWebRequest.Get(mainUri);
                yield return request.SendWebRequest();
                if (string.IsNullOrEmpty(request.downloadHandler.text)
                    || request.isNetworkError
                    || request.isHttpError)
                {
                    GameObject.FindGameObjectsWithTag("MainCanvas")[0].transform.Find("ResetButton").gameObject.SetActive(true);
                    yield break;
                }

                DoString(request.downloadHandler.text, mainUri);

                while (true)
                {
                    yield return new WaitForSeconds(10.0f);
                    DoRequest($"{envUris[selectEnv]}/lua/polling.lua");
                }
            }
        }

        private void Update()
        {
            CurrentState.Collect();
            CurrentState.CheckTop();
        }

        private void OnDestroy()
        {
            CurrentState.Dispose();
            CurrentState = null;
        }

        public void DoString(string luaCode, string name)
        {
            CurrentState.DoString(luaCode, name);
        }

        public void DoFile(string fileName)
        {
            CurrentState.DoFile(fileName);
        }

        public void DoRequest(string uri)
        {
            StartCoroutine(CoRequest(uri, null));
        }

        public void DoRequest(string uri, LuaFunction callback)
        {
            StartCoroutine(CoRequest(uri, callback));
        }

        private IEnumerator CoRequest(string uri, LuaFunction callback)
        {
            var request = UnityWebRequest.Get(uri);
            yield return request.SendWebRequest();
            if (!string.IsNullOrEmpty(request.downloadHandler.text))
            {
                DoString(request.downloadHandler.text, uri);
                if (callback != null)
                {
                    callback.Call();
                    callback.Dispose();
                }
            }
        }

        public void ResetEnv()
        {
            GameObject.FindGameObjectsWithTag("MainCanvas")[0].transform.Find("ResetButton").gameObject.SetActive(false);
            PlayerPrefs.SetInt("env", 0);
            Restart();
        }

        public void Restart()
        {
            LuaAssets.Clear();
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        }

        private Dictionary<string, Coroutine> intervals = new Dictionary<string, Coroutine>();
        public void SetInterval(string id, LuaFunction func, float interval)
        {
            intervals.Add(id, StartCoroutine(CoInterval(func, interval)));
        }

        public void ClearInterval(string id)
        {
            if (intervals.ContainsKey(id))
            {
                StopCoroutine(intervals[id]);
                intervals.Remove(id);
            }
        }

        private IEnumerator CoInterval(LuaFunction func, float interval)
        {
            var wait = new WaitForSeconds(interval);
            while (true)
            {
                func.Call();
                yield return wait;
            }
        }
    }
}
