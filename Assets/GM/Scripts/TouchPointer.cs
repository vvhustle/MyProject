using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class TouchPointer : MonoBehaviour
{
    public ImageParticle particle;
    public Image image;
    public float imageFadeTime;
    public Vector3 imageFadeScale;

    private Vector3 imageOriginScale;

    private void Awake()
    {
        if (image)
        {
            image.DOFade(0.0f, 0.0f);
            imageOriginScale = image.transform.localScale;
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (image)
            {
                image.transform.position = Input.mousePosition;
                image.gameObject.SetActive(true);
                image.DOFade(1.0f, 0.0f);
                image.transform.DOScale(imageOriginScale, 0.0f);
                image.DOFade(0.0f, imageFadeTime);
                image.transform.DOScale(imageFadeScale, imageFadeTime);
            }
        }
        if (Input.GetMouseButton(0))
        {
            if (particle) particle.Play(Input.mousePosition);
        }
        else
        {
            if (particle) particle.Stop();
        }
    }
}
