using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using TMPro;

public class QuestManager : MonoBehaviour
{
    public static QuestManager main;
    public Transform questList;
    public GameObject questItem;
    public Quest[] quests;
    public GameObject buyCost;

    [HideInInspector] public bool buying = false;
    [HideInInspector] public Transform buyingTarget;

    float buyingTimer = 0f;
    float buyingTimerDelay = 1f;
    bool buyingAnimate = false;
    Camera mainCamera;

    void Awake()
    {
        main = this;
    }

    void Start()
    {
        mainCamera = QuestMarker.main.GetComponent<QuestMarker>().mainCamera;
    }

    void Update()
    {
        if (buying)
        {
            buyingTimer += Time.deltaTime;
            buyCost.SetActive(true);

            if (buyCost && !buyingAnimate)
            {
                buyCost.GetComponent<Animator>().SetBool("Buy", true);
                buyingAnimate = true;
            }

            if (buyingTimer >= buyingTimerDelay)
            {
                buying = false;
                buyingAnimate = false;
                buyCost.GetComponent<TextMeshProUGUI>().text = "";
            }

            QuestMarker.main.GetComponent<QuestMarker>().UpdateMarkerPosition(buyingTarget, buyCost.GetComponent<RectTransform>());
        }
        else
        {
            buyCost.SetActive(false);
        }
    }

    public void TaskSetup()
    {
        foreach (Quest item in quests)
        {
            QuestItem questExists = Array.Find(FindObjectsOfType<QuestItem>(), (questItem) => item.id == questItem.idQuest);

            if (!item.complete && questExists == null && item.name != "")
            {
                GameObject prefab = questItem;
                prefab.GetComponent<QuestItem>().text.GetComponent<TextMeshProUGUI>().text = item.name;
                prefab.GetComponent<QuestItem>().nameQuest = item.name;
                prefab.GetComponent<QuestItem>().idQuest = item.id;
                Instantiate(prefab, questList);
                break;
            }
        }
    }

    public void TaskClose(string taskId)
    {
        foreach (Quest item in quests)
        {
            if (item.id == taskId && !item.complete)
            {
                item.complete = true;
                break;
            }
        }

        foreach (QuestItem item in FindObjectsOfType<QuestItem>())
        {
            if (item.idQuest == taskId)
            {
                item.CloseQuest();
                break;
            }
        }
    }
}
