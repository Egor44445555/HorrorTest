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
    Animator buyCostAnimator;
    TextMeshProUGUI buyCostText;
    List<QuestItem> activeQuestItems = new List<QuestItem>();
    QuestMarker questMarker;

    void Awake()
    {
        if (main != null && main != this)
        {
            Destroy(gameObject);
            return;
        }
        
        main = this;
        
        if (buyCost != null)
        {
            buyCostAnimator = buyCost.GetComponent<Animator>();
            buyCostText = buyCost.GetComponent<TextMeshProUGUI>();
        }
    }

    void Start()
    {
        questMarker = QuestMarker.main;

        if (quests.Length > 0 && quests[0].runImmediately)
        {
            TaskSetup();
        }
    }

    void Update()
    {
        if (buying)
        {
            buyingTimer += Time.deltaTime;
            
            if (buyCost != null)
            {
                buyCost.SetActive(true);

                if (buyCostAnimator != null && !buyingAnimate)
                {
                    buyCostAnimator.SetBool("Buy", true);
                    buyingAnimate = true;
                }

                if (buyingTimer >= buyingTimerDelay)
                {
                    buying = false;
                    buyingAnimate = false;
                    
                    if (buyCostText != null)
                    {
                        buyCostText.text = "";
                    }
                }

                if (buyingTarget != null && questMarker != null)
                {
                    questMarker.UpdateMarkerPosition(buyingTarget, buyCost.GetComponent<RectTransform>());
                }
            }
        }
        else if (buyCost != null)
        {
            buyCost.SetActive(false);
        }
    }

    public void TaskSetup()
    {
        QuestItem[] allQuestItems = FindObjectsOfType<QuestItem>();
        
        foreach (Quest quest in quests)
        {
            if (quest.complete || string.IsNullOrEmpty(quest.name))
            {
                continue;
            }

            bool questExists = System.Array.Exists(allQuestItems, item => item.idQuest == quest.id);
            
            if (!questExists)
            {
                CreateQuestItem(quest);
            }

            if (questMarker != null)
            {
                questMarker.SetTarget(quest.target);
            }
            break;
        }
    }

    void CreateQuestItem(Quest quest)
    {
        if (questItem == null || questList == null)
        {
            return;
        }

        GameObject questInstance = Instantiate(questItem, questList);
        QuestItem questItemComponent = questInstance.GetComponent<QuestItem>();
        
        if (questItemComponent != null && questItemComponent.text != null)
        {
            TextMeshProUGUI textComponent = questItemComponent.text.GetComponent<TextMeshProUGUI>();
            if (textComponent != null)
            {
                textComponent.text = quest.name;
            }
            
            questItemComponent.nameQuest = quest.name;
            questItemComponent.idQuest = quest.id;
            activeQuestItems.Add(questItemComponent);
        }
    }

    public void TaskClose(string taskId)
    {
        foreach (Quest quest in quests)
        {
            if (quest.id == taskId && !quest.complete)
            {
                quest.complete = true;
                break;
            }
        }

        for (int i = activeQuestItems.Count - 1; i >= 0; i--)
        {
            if (activeQuestItems[i].idQuest == taskId)
            {
                activeQuestItems[i].CloseQuest();
                activeQuestItems.RemoveAt(i);
                break;
            }
        }

        bool questClosed = activeQuestItems.Exists(item => item.idQuest == taskId);

        if (!questClosed)
        {
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

    void OnDestroy()
    {
        if (main == this)
        {
            main = null;
        }
    }
}