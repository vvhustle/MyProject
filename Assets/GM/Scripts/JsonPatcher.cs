using LuaInterface;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace GM
{
    public class JsonPatcher : MonoBehaviour
    {
        public class FileInfo
        {
            public string md5;
            public int size;
        }

        private string GameDataPath => Path.Combine(Application.persistentDataPath, "GameData");

        public string[] GetFiles()
        {
            if (!Directory.Exists(GameDataPath))
            {
                return new string[] {};
            }
            var files = Directory.GetFiles(GameDataPath);
            var list = new List<string>();
            foreach (var file in files)
            {
                list.Add(Path.GetFileNameWithoutExtension(file));
            }
            return list.ToArray();
        }

        public string ReadFile(string filename)
        {
            return File.ReadAllText(Path.Combine(GameDataPath, $"{filename}.json"));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uri">md5 전체 주소</param>
        /// <param name="callback">업데이트 필요 string[]을 콜백합니다.</param>
        public void CheckMD5(string uri, LuaFunction callback)
        {
            StartCoroutine(CoCheckMD5(uri, callback));
        }

        private IEnumerator CoCheckMD5(string uri, LuaFunction callback)
        {
            var req = UnityWebRequest.Get(uri);
            yield return req.SendWebRequest();

            var serverMD5 = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, FileInfo>>(req.downloadHandler.text);
            var clientMD5 = GetClientMD5();

            var list = new List<string>();
            int size = 0;
            foreach (var pair in serverMD5)
            {
                if (!clientMD5.ContainsKey(pair.Key)
                    || serverMD5[pair.Key].md5 != clientMD5[pair.Key])
                {
                    list.Add(pair.Key);
                    size += serverMD5[pair.Key].size;
                }
            }
            callback.Call(list.ToArray(), size);
            callback.Dispose();
            req.Dispose();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uri">업데이트 폴더 url, 파일명은 붙이지않은</param>
        /// <param name="countCallback">파일이 하나씩 업데이트 될때마다 currnet, max를 콜백합니다.</param>
        /// <param name="completeCallback">전체 업데이트가 완료되면 콜백합니다.</param>
        /// <param name="errorCallback">에러가 발생하면 콜백합니다.</param>
        public void UpdateData(string uri, string[] files, LuaFunction countCallback, LuaFunction completeCallback, LuaFunction errorCallback)
        {
            StartCoroutine(CoUpdateData(uri, files, countCallback, completeCallback, errorCallback));// $"{uri}/{pair.Key}.json", $"{pair.Key}.json"));
        }

        private IEnumerator CoUpdateData(string uri, string[] files, LuaFunction countCallback, LuaFunction completeCallback, LuaFunction errorCallback)
        {
            if (!Directory.Exists(GameDataPath))
            {
                Directory.CreateDirectory(GameDataPath);
            }

            int count = 0;
            countCallback.Call(count, files.Length);
            foreach (var file in files)
            {
                var req = UnityWebRequest.Get($"{uri}/{file}.json");
                yield return req.SendWebRequest();
                if (req.isHttpError)
                {
                    errorCallback.Call(req.error);
                    errorCallback.Dispose();
                    req.Dispose();
                    yield break;
                }
                File.WriteAllText(Path.Combine(GameDataPath, $"{file}.json"), req.downloadHandler.text);
                countCallback.Call(count, files.Length);
                count++;
                req.Dispose();
            }
            countCallback.Dispose();
            completeCallback.Call();
            completeCallback.Dispose();
        }

        private Dictionary<string, string> GetClientMD5()
        {
            var data = new Dictionary<string, string>();
            if (!Directory.Exists(GameDataPath))
            {
                return data;
            }
            var files = Directory.GetFiles(GameDataPath);
            foreach (var filename in files)
            {
                data.Add(Path.GetFileNameWithoutExtension(filename), CreateMD5(File.ReadAllText(filename)));
            }
            return data;
        }

        private string CreateMD5(string input)
        {
            byte[] encodedInput = new UTF8Encoding().GetBytes(input);
            byte[] hash = ((HashAlgorithm)CryptoConfig.CreateFromName("MD5")).ComputeHash(encodedInput);
            return System.BitConverter.ToString(hash).Replace("-", string.Empty).ToLower();
        }
    }
}
