using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskItem : MonoBehaviour
{
    public string taskId;
    public bool disposable = false;
    public Transform taskTarget;
    public float offsetX = 0f;
    public float offsetY = 0f;

    [HideInInspector] public bool completed = false;

    bool newTask = false;
    QuestMarker questMarker;

    void Start()
    {
        questMarker = QuestMarker.main;
    }

    void Update()
    {
        if (completed)
        {
            foreach (Quest item in QuestManager.main.quests)
            {
                if (item.id == taskId && !item.complete)
                {
                    item.complete = true;
                    questMarker.SetTarget(item.target);
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

            if (!newTask)
            {
                QuestManager.main.TaskSetup();
                newTask = true;
            }            
        }
    }
}
