using LuaInterface;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace GM
{
    public class LuaRequest : MonoBehaviour
    {
        public void Send(string uri, string data, LuaFunction callback)
        {
            var formFields = new Dictionary<string, string>
            {
                { "data", data }
            };
            StartCoroutine(CoRequest(UnityWebRequest.Post(uri, formFields), callback));
        }

        private IEnumerator CoRequest(UnityWebRequest request, LuaFunction callback)
        {
            yield return request.SendWebRequest();
            if (request.isNetworkError)
            {
            }
            else
            {
                if (callback != null)
                {
                    callback.Call(request.downloadHandler.text);
                    callback.Dispose();
                }
            }
            request.Dispose();
            request = null;
            Destroy(this);
        }
    }
}
