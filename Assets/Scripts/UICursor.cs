using UnityEngine;
using UnityEngine.UI;

public class UICursor : MonoBehaviour
{
    public static UICursor main;
    [SerializeField] Sprite aimCursor;
    [SerializeField] Vector2 aimCursorSize;
    [SerializeField] Sprite grabCursor;
    [SerializeField] Vector2 grabCursorSize;

    Image image;
    RectTransform rectTransform;
    
    void Awake()
    {
        if (main == null)
        {
            main = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        image = GetComponent<Image>();
    }

    public void GrabCursor(bool grab)
    {
        if (grab)
        {
            image.sprite = grabCursor;
            rectTransform.sizeDelta = new Vector2(grabCursorSize.x, grabCursorSize.y);
        }
        else
        {
            image.sprite = aimCursor;
            rectTransform.sizeDelta = new Vector2(aimCursorSize.x, aimCursorSize.y);
        }
    }

    void OnDestroy()
    {
        if (main == this)
        {
            main = null;
        }
    }
}
