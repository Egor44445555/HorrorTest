using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskZone : MonoBehaviour
{
    bool taskInZone = false;
    GameObject taskItem;
    GameObject cam;
    TaskPlase parentTaskPlase;
    float lastForceTime = 0f;

    void Start()
    {
        if (PlayerController.main != null)
        {
            cam = PlayerController.main.GetCam();
        }

        parentTaskPlase = transform.parent != null ? transform.parent.GetComponent<TaskPlase>() : null;
    }

    void OnTriggerEnter(Collider other)
    {
        TaskItem taskItemComponent = other.GetComponent<TaskItem>();

        if (taskItemComponent == null) return;

        if (IsValidItem(other.gameObject))
        {
            taskInZone = true;
            taskItem = other.gameObject;
            
            if (PlayerController.main != null)
            {
                PlayerController.main.DropObject();
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject == taskItem)
        {
            taskInZone = false;
            taskItem = null;
        }
    }

    bool IsValidItem(GameObject item)
    {
        if (parentTaskPlase == null || parentTaskPlase.currentObject == null)
        {
            return true;
        }

        TaskItem itemTaskComponent = item.GetComponent<TaskItem>();

        if (itemTaskComponent == null) return false;

        foreach (GameObject validObject in parentTaskPlase.currentObject)
        {
            if (validObject == null) continue;

            if (validObject.CompareTag(item.tag))
            {
                return true;
            }

            TaskItem validTaskItem = validObject.GetComponent<TaskItem>();

            if (validTaskItem != null && validTaskItem.GetTaskId() == itemTaskComponent.GetTaskId())
            {
                return true;
            }
        }

        return false;
    }

    public bool IsInTaskInZone()
    {
        return taskInZone;
    }

    public GameObject GetTaskItem()
    {
        return taskItem;
    }

    

    void OnDestroy()
    {
        taskItem = null;
    }
}