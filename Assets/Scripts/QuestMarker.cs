using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class QuestMarker : MonoBehaviour
{
    public static QuestMarker main;
    [SerializeField] Transform markerPlace;
    [SerializeField] Sprite icon;
    public Camera mainCamera;

    [HideInInspector] public Transform target;

    RectTransform markerRect;
    Image markerImage;

    void Awake()
    {
        if (main != null && main != this)
		{
			Destroy(gameObject);
			return;
		}
		
		main = this;
    }

    void Start()
    {
        GameObject markerObj = new GameObject("Quest Marker");
        markerObj.tag = "Marker";
        markerRect = markerObj.AddComponent<RectTransform>();
        markerImage = markerObj.AddComponent<Image>();
        markerRect.sizeDelta = new Vector2(40f, 40f);

        markerImage.sprite = icon;
        markerRect.SetParent(markerPlace);
        markerRect.localScale = Vector3.one;
    }

    void Update()
    {
        if (target == null)
        {
            if (FindObjectOfType<QuestItem>() && !PlayerController.main.isHolding)
            {
                Quest quest = Array.Find(QuestManager.main.quests, (item) => item.id == FindObjectOfType<QuestItem>().idQuest);
                target = quest.target;
            }

            if (GameObject.FindWithTag("Marker"))
            {
                markerImage.enabled = false;
            }
            
            return;
        }

        UpdateMarkerPosition(target, markerRect);
    }

    public void UpdateMarkerPosition(Transform _target, RectTransform _markerRect)
    {
        Vector3 targetScreenPos = mainCamera.WorldToScreenPoint(_target.position);

        bool isOffScreen = targetScreenPos.z <= 0 ||
                          targetScreenPos.x <= 0 ||
                          targetScreenPos.x >= Screen.width ||
                          targetScreenPos.y <= 0 ||
                          targetScreenPos.y >= Screen.height;

        if (isOffScreen)
        {
            markerImage.enabled = true;
            targetScreenPos = GetScreenEdgePosition(_target.position);
        }
        else
        {
            markerImage.enabled = true;
        }

        _markerRect.position = targetScreenPos;
    }

    public Vector3 GetScreenEdgePosition(Vector3 worldPos)
    {
        Vector3 screenCenter = new Vector3(Screen.width, Screen.height, 0) / 2;
        Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPos);

        if (screenPos.z < 0) screenPos *= -1;

        screenPos -= screenCenter;

        float angle = Mathf.Atan2(screenPos.y, screenPos.x);
        angle -= 90 * Mathf.Deg2Rad;

        float cos = Mathf.Cos(angle);
        float sin = -Mathf.Sin(angle);

        float m = cos / sin;

        Vector3 screenBounds = screenCenter * 0.9f;

        if (cos > 0)
        {
            screenPos = new Vector3(screenBounds.y / m, screenBounds.y, 0);
        }
        else
        {
            screenPos = new Vector3(-screenBounds.y / m, -screenBounds.y, 0);
        }

        if (screenPos.x > screenBounds.x)
        {
            screenPos = new Vector3(screenBounds.x, screenBounds.x * m, 0);
        }
        else if (screenPos.x < -screenBounds.x)
        {
            screenPos = new Vector3(-screenBounds.x, -screenBounds.x * m, 0);
        }

        screenPos += screenCenter;
        return screenPos;
    }

    void OnDestroy()
    {
        icon = null;
        markerImage = null;
        target = null;

        if (main == this)
        {
            main = null;
        }

        if (markerRect != null)
        {
            Destroy(markerRect.gameObject);
            markerRect = null;
        }
    }
}