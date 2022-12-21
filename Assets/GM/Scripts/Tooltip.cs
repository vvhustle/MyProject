using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Tooltip : MonoBehaviour
{
    private static Tooltip _instance;

    public static void ShowText(string text)
    {
        _instance.ShowText_(text);
        _instance.SetPosition();
    }

    private void Awake()
    {
        _instance = this;
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var rect = GetComponent<RectTransform>();
            rect.sizeDelta = transform.GetChild(0).GetComponent<RectTransform>().sizeDelta;
            var pos = rect.anchoredPosition;
            pos.x = Screen.width + rect.sizeDelta.x + 99.0f;
            pos.y = Screen.height + rect.sizeDelta.y + 99.0f;
            rect.anchoredPosition = pos;
            gameObject.SetActive(false);
        }
    }

    private void ShowText_(string text)
    {
        gameObject.SetActive(true);
        transform.GetChild(0).gameObject.SetActive(true);
        var texts = GetComponentsInChildren<Text>();
        foreach (var txt in texts)
            txt.text = text;
    }

    private void SetPosition()
    {
        StartCoroutine(CoSetPosition());
    }

    private IEnumerator CoSetPosition()
    {
        yield return null;

        var canvas = transform.root.GetComponent<Canvas>();
        var width = Screen.width;
        var height = Screen.height;

        var rect = GetComponent<RectTransform>();
        rect.sizeDelta = transform.GetChild(0).GetComponent<RectTransform>().sizeDelta * canvas.scaleFactor;

        var pos = Input.mousePosition;
        var sizeHalf = rect.sizeDelta.x * 0.5f;
        if (pos.x - (sizeHalf) < -width * 0.5f)
            pos.x += sizeHalf;
        if (pos.x + (sizeHalf) > width * 0.5f)
            pos.x -= sizeHalf;

        rect.position = pos;
    }
}
