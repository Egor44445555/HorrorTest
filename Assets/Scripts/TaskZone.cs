using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TaskZone : MonoBehaviour
{
    [HideInInspector] public bool taskInZone = false;
    // [HideInInspector]
    public GameObject taskItem;

    public bool inZone = false;
    GameObject cam;

    void Start()
    {
        if (PlayerController.main != null)
        {
            cam = PlayerController.main.GetCam();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<TaskItem>() && !inZone)
        {
            taskInZone = true;
            taskItem = other.gameObject;
            PlayerController.main.DropObject();
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (transform.parent != null && transform.parent.GetComponent<TaskPlase>())
        {            
            GameObject currentItemObj = Array.Find(transform.parent.GetComponent<TaskPlase>().currentObject, (item) => item.name == other.gameObject.name);

            if (currentItemObj == null && other.gameObject.GetComponent<Rigidbody>())
            {
                other.gameObject.GetComponent<Rigidbody>().AddForce(cam.transform.forward * -0.1f, ForceMode.Impulse);
            }
        }

        if (other.gameObject.GetComponent<TaskItem>())
        {
            inZone = true;
        }
        else
        {
            inZone = false;
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.GetComponent<TaskItem>())
        {
            taskInZone = false;
            taskItem = null;
        }
    }
}
