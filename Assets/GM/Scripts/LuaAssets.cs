using LuaInterface;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace GM
{
    public class LuaAssets : MonoBehaviour
    {
        private static LuaAssets Instance;
        private static Dictionary<string, AssetBundle> Bundles = new Dictionary<string, AssetBundle>();

        private void Awake()
        {
            Instance = this;
        }

        public static void Clear()
        {
            AssetBundle.UnloadAllAssetBundles(true);
            Bundles.Clear();
        }

        public static void Load(string uri, LuaFunction callback)
        {
            Instance.StartCoroutine(Instance.CoLoad(uri, callback, 0));
        }

        public static void Load(string uri, LuaFunction callback, uint version)
        {
            Instance.StartCoroutine(Instance.CoLoad(uri, callback, version));
        }

        private IEnumerator CoLoad(string uri, LuaFunction callback, uint version)
        {
            if (Bundles.ContainsKey(uri))
            {
                callback?.Call();
                yield break;
            }

            UnityWebRequest req;
            if (version == 0)
            {
                req = UnityWebRequestAssetBundle.GetAssetBundle(uri);
            }
            else
            {
                req = UnityWebRequestAssetBundle.GetAssetBundle(uri, version, 0);
            }
            yield return req.SendWebRequest();
            AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(req);
            if (bundle != null)
            {
                Bundles.Add(uri, bundle);
            }
            callback?.Call();
        }

        private static AssetBundle FindBundle(string name)
        {
            // find with bundle name
            foreach (var pair in Bundles)
            {
                if (pair.Value == null)
                {
                    continue;
                }

                if (name == pair.Value.name)
                {
                    return pair.Value;
                }
            }

            var sub = name.LastIndexOf('/');
            if (sub > 0)
            {
                name = name.Substring(0, name.LastIndexOf('/'));
            }

            // find with bundle split name
            foreach (var pair in Bundles)
            {
                if (pair.Value == null)
                {
                    continue;
                }

                if (name == pair.Value.name)
                {
                    return pair.Value;
                }
            }

            // find with asset name
            foreach (var pair in Bundles)
            {
                if (pair.Value == null)
                {
                    continue;
                }

                if (pair.Value.LoadAsset(name))
                {
                    return pair.Value;
                }
            }
            return null;
        }

        public static GameObject Instantiate(string name)
        {
            return Instantiate(name, null);
        }

        public static GameObject Instantiate(string name, Transform parent)
        {
            var bundle = FindBundle(name);
            var names = name.Split('/');
            if (names.Length == 2)
            {
                name = names[1];
            }
            if (bundle == null)
                return null;
            var go = bundle.LoadAsset<GameObject>(name);
            if (go)
            {
                var clone = Instantiate(go, parent);
                clone.name = name;
                return clone;
            }
            return null;
        }

        public static string LoadText(string name)
        {
            var bundle = FindBundle(name);
            var names = name.Split('/');
            if (names.Length == 2)
            {
                name = names[1];
            }
            var asset = bundle.LoadAsset<TextAsset>(name);
            if (asset)
            {
                return asset.text;
            }
            return "";
        }

        public static Sprite LoadSprite(string name)
        {
            var bundle = FindBundle(name);
            if (bundle == null)
            {
                return null;
            }

            var names = name.Split('/');
            if (names.Length == 3)
            {
                var assetOne = bundle.LoadAsset<Sprite>(names[2]);
                if (assetOne != null)
                {
                    return assetOne;
                }
                var assets = bundle.LoadAssetWithSubAssets<Sprite>(names[1]);
                foreach (var asset in assets)
                {
                    if (asset.name == names[2])
                    {
                        return asset;
                    }
                }
            }
            else if (names.Length == 2)
            {
                var asset = bundle.LoadAsset<Sprite>(names[1]);
                if (asset)
                {
                    return asset;
                }
            }
            else
            {
                var asset = bundle.LoadAsset<Sprite>(name);
                if (asset)
                {
                    return asset;
                }
            }
            return null;
        }
    }
}
