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
    
    List<QuestItem> activeQuestItems = new List<QuestItem>();

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
        if (quests.Length > 0 && quests[0].runImmediately)
        {
            TaskSetup();
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

            if (QuestMarker.main != null)
            {
                QuestMarker.main.SetTarget(quest.target);
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