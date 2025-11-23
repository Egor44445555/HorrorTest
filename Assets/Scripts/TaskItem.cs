using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskItem : MonoBehaviour
{
    [SerializeField] string taskId;
    [SerializeField] bool disposable = false;
    [SerializeField] Transform taskTarget;
    [SerializeField] float offsetX = 0f;
    [SerializeField] float offsetY = 0f;

    bool completed = false;
    bool hasProcessedCompletion = false;

    void Update()
    {
        if (completed && !hasProcessedCompletion)
        {
            ProcessCompletion();
        }
    }

    void ProcessCompletion()
    {
        if (hasProcessedCompletion) return;

        if (QuestManager.main == null)
        {
            return;
        }

        bool questFound = false;

        foreach (Quest item in QuestManager.main.quests)
        {
            if (item.id == taskId && !item.complete)
            {
                item.complete = true;
                questFound = true;
                
                if (QuestMarker.main != null && item.target != null)
                {
                    QuestMarker.main.SetTarget(item.target);
                }
                break;
            }
        }

        CloseQuestItem();
        QuestManager.main.TaskSetup();        
        hasProcessedCompletion = true;
    }

    void CloseQuestItem()
    {
        QuestItem[] questItems = FindObjectsOfType<QuestItem>();
        bool itemClosed = false;

        foreach (QuestItem item in questItems)
        {
            if (item.idQuest == taskId)
            {
                item.CloseQuest();
                itemClosed = true;
                break;
            }
        }
    }

    public void SetCompleted()
    {
        completed = true;
    }

    public bool IsCompleted()
    {
        return completed;
    }

    public bool IsDisposable()
    {
        return disposable;
    }

    public Transform GetTaskTarget()
    {
        return taskTarget;
    }

    public string GetTaskId()
    {
        return taskId;
    }

    void OnDestroy()
    {
        if (QuestMarker.main != null && QuestMarker.main.GetCurrentTarget() == taskTarget)
        {
            QuestMarker.main.ClearTarget();
        }
    }
}