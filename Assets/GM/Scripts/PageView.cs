using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PageView : MonoBehaviour, IBeginDragHandler, IEndDragHandler
{
    [SerializeField]
    private bool interactable = true;
    public bool Interactable
    {
        get { return interactable; }
        set
        {
            interactable = value;
            if (scrollRect != null) scrollRect.horizontal = Interactable;
        }
    }

    [Tooltip("page movement rate")]
    public float smooting = 5f;
    [Tooltip("How much is successful?")]
    [Range(0.1f, 1f)]
    public float limit = 0.3f;

    public bool autoPlay = false;
    public float autoPlaySeconds = 5f;

    //ScrollRect component on this game object
    private ScrollRect scrollRect;

    private int totalPage = 0; //The number of pages currently created

    private int selectPage = 0; // currently selected page

    private bool isDrag = false; // is currently dragging

    private float targetHorizontalPosition; //current target horizontal position

    private Vector2 rectSize = Vector2.zero; //Window size
 
    void Awake()
    {
        scrollRect = GetComponent<ScrollRect>();
        if (scrollRect == null || !scrollRect.enabled)
        {
            Debug.LogWarning("PageView Component Need Activity ScrollRect !");
            enabled = false;
            return;
        }
        //ScrollRect initialization settings
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.vertical = false;
        scrollRect.horizontal = interactable;
        // Re - add Content to the ContentFitter and Layout components
        DestroyImmediate(scrollRect.content.gameObject.GetComponent<ContentSizeFitter>());
        DestroyImmediate(scrollRect.content.gameObject.GetComponent<HorizontalLayoutGroup>());
        var size_fitter = scrollRect.content.gameObject.AddComponent<ContentSizeFitter>();
        var layout_group = scrollRect.content.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout_group.childControlHeight = true;
        layout_group.childForceExpandHeight = true;
        layout_group.childControlWidth = false;
        layout_group.childForceExpandWidth = false;
        layout_group.childAlignment = TextAnchor.MiddleLeft;
        size_fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

    }

    // Update is called once per frame
    private float updateTime = 0;
    void Update()
    {
                 // Check whether there is a node change(only by the number of nodes to determine, if you manually change the node<number unchanged>, you need to manually call the following method)
        if (totalPage != scrollRect.content.childCount)
        {
            InitializationPage();
        }
        //No dragging
        if (!isDrag && totalPage > 0)
        {
            //m_ScrollRect.horizontalNormalizedPosition = Mathf.Lerp(m_ScrollRect.horizontalNormalizedPosition, targetHorizontal, Time.deltaTime * smooting);
            scrollRect.content.localPosition = new Vector3(Mathf.Lerp(scrollRect.content.localPosition.x, targetHorizontalPosition, Time.deltaTime * smooting), 0, 0);
        }
        //Autoplay 
        if (autoPlay && totalPage > 0)
        {
            if (!isDrag)
            {
                updateTime += Time.deltaTime;
                if (updateTime >= autoPlaySeconds)
                {
                    updateTime = 0f;
                    MoveLeft();
                }
            }
            else
            {
                updateTime = 0f;
            }
        }
    }

    /// <summary>
    /// If you manually change the node (the number does not change), you need to call this method manually, and you can't have Active=false and non-RectTransform nodes.
    /// \n This method can only be called after the second frame is instantiated, otherwise the window size may not be obtained.
    /// </summary>
    public void InitializationPage()
    {
        // Get the window size
        rectSize = scrollRect.viewport.rect.size;
        if (rectSize.x == 0 || rectSize.y == 0)
        {
            return;
        }
        // Initialize the child node
        foreach (RectTransform trf in scrollRect.content)
        {
            trf.gameObject.SetActive(true);
            trf.sizeDelta = rectSize;
        }
        totalPage = scrollRect.content.childCount;
        Compute();
        //TODO callback method
    }

    /// <summary>
    /// Move to the left, if it is already the last one, loop to the first
    /// </summary>
    public void MoveLeft(bool loop = true)
    {
        selectPage += 1;
        if (loop && selectPage > totalPage)
        {
            selectPage = 0;
        }
        Compute();
    }

    /// <summary>
    /// Move to the right, if it is already the first one, loop to the last one
    /// </summary>
    public void MoveRight(bool loop = true)
    {
        selectPage -= 1;
        if (loop && selectPage < 0) selectPage = totalPage;
        Compute();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        //Debug.Log("Begin");
        isDrag = true;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        //Debug.Log("End");
        isDrag = false;
        // window size
        if (rectSize.x == 0 || rectSize.y == 0) return;
        // Calculate the sliding result
        var offsetWidth = -(selectPage - 1) * rectSize.x - scrollRect.content.localPosition.x; //The difference between the actual position and the expected position
        if (Mathf.Abs(offsetWidth) > rectSize.x * limit)
        {
            //Swipe right
            if (offsetWidth < 0)
            {
                selectPage -= 1;
            }
            //Swipe left
            else
            {
                selectPage += 1;
            }
            Compute();
        }
    }
 
    // Calculate the value of targetHorizontalPosition
    private void Compute()
    {
        // window size
        if (rectSize.x == 0 || rectSize.y == 0)
        {
            return;
        }
        //Calculated
        selectPage = totalPage <= 0 ? 0 : selectPage > totalPage ? totalPage : selectPage <= 0 ? 1 : selectPage;
        targetHorizontalPosition = -(selectPage - 1) * rectSize.x;
    }
}
