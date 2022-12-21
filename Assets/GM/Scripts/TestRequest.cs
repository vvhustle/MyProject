using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestRequest : MonoBehaviour
{
    public string uri;
    public string data;

    void Start()
    {
        var btn = GetComponent<Button>();
        btn.onClick.AddListener(() =>
        {
            StartCoroutine(CoRequest());
        });
    }

    IEnumerator CoRequest()
    {
        var form = new Dictionary<string, string>
        {
            { "data", data }
        };
        var req = UnityEngine.Networking.UnityWebRequest.Post(uri, form);
        yield return req.SendWebRequest();

        Debug.Log(req.downloadHandler.text);
    }
}
