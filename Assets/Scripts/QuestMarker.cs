using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class QuestMarker : MonoBehaviour
{
    public static QuestMarker main;
    [SerializeField] Transform markerPlace;
    [SerializeField] Sprite icon;
    [SerializeField] Camera mainCamera;

    Transform target;
    RectTransform markerRect;
    Image markerImage;
    QuestItem currentQuestItem;
    bool isInitialized = false;

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
        InitializeMarker();
    }

    void InitializeMarker()
    {
        if (isInitialized) return;

        GameObject markerObj = new GameObject("Quest Marker");
        markerObj.tag = "Marker";
        markerRect = markerObj.AddComponent<RectTransform>();
        markerImage = markerObj.AddComponent<Image>();
        markerRect.sizeDelta = new Vector2(20f, 20f);

        if (icon != null)
        {
            markerImage.sprite = icon;
        }

        if (markerPlace != null)
        {
            markerRect.SetParent(markerPlace);
        }
        else
        {
            markerRect.SetParent(transform);
        }
        
        markerRect.localScale = Vector3.one;
        markerImage.enabled = false;
        isInitialized = true;
    }

    void Update()
    {
        if (!isInitialized) return;

        if (target == null)
        {
            UpdateTargetFromActiveQuest();
        }

        if (target != null && markerImage != null)
        {
            UpdateMarkerPosition(target, markerRect);
        }
    }

    void UpdateTargetFromActiveQuest()
    {
        if (currentQuestItem == null)
        {
            currentQuestItem = FindObjectOfType<QuestItem>();
        }

        if (currentQuestItem != null && CanShowMarker())
        {
            Quest quest = FindQuestById(currentQuestItem.idQuest);
            if (quest != null && quest.target != null)
            {
                target = quest.target;
                if (markerImage != null)
                {
                    markerImage.enabled = true;
                }
            }
        }
        else if (markerImage != null)
        {
            markerImage.enabled = false;
        }
    }

    bool CanShowMarker()
    {
        if (PlayerController.main == null) return true;
        
        return !PlayerController.main.isHolding;
    }

    Quest FindQuestById(string questId)
    {
        if (QuestManager.main == null || QuestManager.main.quests == null)
        {
            return null;
        }

        return Array.Find(QuestManager.main.quests, item => item.id == questId);
    }

    public void UpdateMarkerPosition(Transform _target, RectTransform _markerRect)
    {
        if (_target == null || _markerRect == null || mainCamera == null)
            return;

        Vector3 targetScreenPos = mainCamera.WorldToScreenPoint(_target.position);

        bool isOffScreen = targetScreenPos.z <= 0 ||
                        targetScreenPos.x <= 0 ||
                        targetScreenPos.x >= Screen.width ||
                        targetScreenPos.y <= 0 ||
                        targetScreenPos.y >= Screen.height;

        if (isOffScreen)
        {
            targetScreenPos = GetScreenEdgePosition(_target.position);
        }

        _markerRect.position = targetScreenPos;
        
        if (markerImage != null)
        {
            markerImage.enabled = true;
        }
    }

    public Vector3 GetScreenEdgePosition(Vector3 worldPos)
    {
        if (mainCamera == null)
            return Vector3.zero;

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

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;

        if (markerImage != null)
        {
            markerImage.enabled = newTarget != null;
        }
    }

    public Transform GetCurrentTarget()
    {
        return target;
    }

    public void ClearTarget()
    {
        target = null;
        currentQuestItem = null;

        if (markerImage != null)
        {
            markerImage.enabled = false;
        }
    }

    void OnDestroy()
    {
        if (main == this)
        {
            main = null;
        }

        if (markerRect != null)
        {
            Destroy(markerRect.gameObject);
        }
    }
}