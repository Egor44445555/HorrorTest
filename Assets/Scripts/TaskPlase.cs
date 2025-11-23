using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskPlase : MonoBehaviour
{
    public Rigidbody pointObject;
    public float taskDelay = 4f;
    public GameObject taskZone;
    public string completeTaskId;
    public Transform nextMarkerPoint;
    public GameObject[] currentObject;
    public bool hideZoneAfterTask = false;

    private bool itemPlaced = false;
    private float timer = 0f;
    private bool startCupFillingAnim = false;
    private AudioSource audioSource;
    private GameObject cam;
    private GameObject finishedItem;
    private TaskZone taskZoneComponent;
    private bool hasProcessedCompletion = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        taskZoneComponent = taskZone != null ? taskZone.GetComponent<TaskZone>() : null;
        
        if (PlayerController.main != null)
        {
            cam = PlayerController.main.GetCam();
        }
    }

    void Update()
    {
        if (taskZoneComponent == null) return;

        timer += Time.deltaTime;

        ProcessTaskZoneInteraction();
        ProcessTaskCompletion();
    }

    void ProcessTaskZoneInteraction()
    {
        if (itemPlaced || !taskZoneComponent.IsInTaskInZone() || taskZoneComponent.GetTaskItem() == null)
        {
            return;
        }

        GameObject taskItem = taskZoneComponent.GetTaskItem();

        if (!IsValidItem(taskItem))
        {
            return;
        }

        PlaceItem(taskItem);
    }

    bool IsValidItem(GameObject taskItem)
    {
        if (currentObject == null || currentObject.Length == 0)
        {
            return true;
        }

        foreach (GameObject validObject in currentObject)
        {
            if (validObject.CompareTag(taskItem.tag))
            {
                return true;
            }
            
            TaskItem validTaskItem = validObject.GetComponent<TaskItem>();
            TaskItem currentTaskItem = taskItem.GetComponent<TaskItem>();

            if (validTaskItem != null && currentTaskItem != null && validTaskItem.GetTaskId() == currentTaskItem.GetTaskId())
            {
                return true;
            }
        }
        
        return false;
    }

    void PlaceItem(GameObject taskItem)
    {
        Rigidbody rb = taskItem.GetComponent<Rigidbody>();

        if (rb == null) return;

        if (PlayerController.main != null)
        {
            PlayerController.main.DropObject();
        }

        rb.isKinematic = true;
        rb.position = pointObject.position;
        rb.rotation = pointObject.rotation;
        
        itemPlaced = true;
        timer = 0f;
        finishedItem = taskItem;

        if (audioSource != null)
        {
            audioSource.Play();
        }

        TaskItem taskItemComponent = taskItem.GetComponent<TaskItem>();

        if (taskItemComponent != null && taskItemComponent.IsDisposable())
        {
            taskItem.tag = "Untagged";
        }

        Item itemComponent = taskItem.GetComponent<Item>();

        if (itemComponent != null && !startCupFillingAnim)
        {
            itemComponent.anim.SetBool("Filling", true);
            itemComponent.FillCup();
            startCupFillingAnim = true;
        }
    }

    void ProcessTaskCompletion()
    {
        if (!itemPlaced || hasProcessedCompletion) return;

        if (timer >= taskDelay)
        {
            CompleteTask();
            hasProcessedCompletion = true;
        }
    }

    void CompleteTask()
    {
        if (audioSource != null)
        {
            audioSource.Stop();
        }

        if (hideZoneAfterTask && taskZone != null)
        {
            taskZone.SetActive(false);
        }

        if (!string.IsNullOrEmpty(completeTaskId))
        {
            ProcessQuestCompletion();
        }
    }

    void ProcessQuestCompletion()
    {
        if (QuestManager.main == null)
        {
            return;
        }

        foreach (Quest quest in QuestManager.main.quests)
        {
            if (quest.id == completeTaskId && !quest.complete)
            {
                quest.complete = true;
                
                if (QuestMarker.main != null && nextMarkerPoint != null)
                {
                    QuestMarker.main.SetTarget(nextMarkerPoint);
                }
                break;
            }
        }

        QuestManager.main.TaskClose(completeTaskId);
        QuestManager.main.TaskSetup();
    }

    void OnDestroy()
    {
        if (QuestMarker.main != null && QuestMarker.main.GetCurrentTarget() == nextMarkerPoint)
        {
            QuestMarker.main.ClearTarget();
        }
    }
}