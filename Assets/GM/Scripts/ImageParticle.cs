
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ImageParticle : MonoBehaviour
{
    public bool playOnStart;
    public float interval;
    public int countPerTick;
    public float positionSize;
    public float moveSpeed;
    public float moveTime;
    public float fadeTime;

    private bool playing;
    private float updateInterval;
    private Vector3 position;

    public void Play(Vector3 position)
    {
        playing = true;
        this.position = position;
    }

    private void FixedUpdate()
    {
        if (playing)
        {
            updateInterval += Time.deltaTime;
            if (updateInterval >= interval)
            {
                updateInterval -= interval;
                var count = 0;
                for (int i = 0; i < transform.childCount; ++i)
                {
                    var child = transform.GetChild(i);
                    if (child.gameObject.activeSelf)
                        continue;

                    count++;
                    child.gameObject.SetActive(true);
                    child.position = position + new Vector3(Random.Range(-positionSize, positionSize), Random.Range(-positionSize, positionSize), 0.0f); ;
                    var moveBy = new Vector3(Random.Range(-moveSpeed, moveSpeed), Random.Range(-moveSpeed, moveSpeed), 0.0f);
                    child.DOBlendableLocalMoveBy(moveBy, moveTime).onComplete = () =>
                    {
                        child.gameObject.SetActive(false);
                    };
                    if (fadeTime > 0.0f)
                    {
                        var img = child.GetComponent<Image>();
                        img.DOFade(1.0f, 0.0f);
                        img.DOFade(0.0f, fadeTime);
                    }
                    if (count >= countPerTick)
                        break;
                }
            }
        }
    }

    public void Stop()
    {
        playing = false;
    }

    private void Awake()
    {
        for (int i = 0; i < transform.childCount; ++i)
        {
            transform.GetChild(i).gameObject.SetActive(false);
        }
    }

    private void Start()
    {
        if (playOnStart)
        {
            Play(transform.position);
        }
    }
}
