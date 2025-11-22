using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

public class TaskPlase : MonoBehaviour
{
    public Rigidbody pointObject;
    public float taskDelay = 4f;
    public GameObject taskZone;
    public string completeTaskId;
    public Transform nextMarkerPoint;
    public GameObject[] currentObject;
    public bool hideZoneAfterTask = false;

    bool itemPlaced = false;
    float timer = 0f;
    bool startCupFillingAnim = false;
    AudioSource audioSource;
    GameObject cam;
    GameObject finishedItem;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        
        if (PlayerController.main != null)
        {
            cam = PlayerController.main.GetCam();
        }
    }

    void Update()
    {
        timer += Time.deltaTime;
        bool currentItem = false;

        if (taskZone.GetComponent<TaskZone>().taskInZone && taskZone.GetComponent<TaskZone>().taskItem)
        {
            GameObject currentItemObj = Array.Find(currentObject, (item) => item.name == taskZone.GetComponent<TaskZone>().taskItem.name);
            finishedItem = taskZone.GetComponent<TaskZone>().taskItem;

            if (currentItemObj != null)
            {
                currentItem = true;
            }
            else if (taskZone.GetComponent<TaskZone>().taskItem.GetComponent<Rigidbody>() != null)
            {
                taskZone.GetComponent<TaskZone>().taskItem.GetComponent<Rigidbody>().AddForce(cam.transform.forward * -0.1f, ForceMode.Impulse);
            }
        }

        if (taskZone.GetComponent<TaskZone>().taskInZone && taskZone.GetComponent<TaskZone>().taskItem != null && !itemPlaced && currentItem)
        {
            taskZone.GetComponent<TaskZone>().taskItem.GetComponent<Rigidbody>().isKinematic = true;
            taskZone.GetComponent<TaskZone>().taskItem.GetComponent<Rigidbody>().position = pointObject.position;
            taskZone.GetComponent<TaskZone>().taskItem.GetComponent<Rigidbody>().rotation = pointObject.rotation;
            itemPlaced = true;
            timer = 0f;

            if (audioSource != null)
            {
                audioSource.Play();
            }

            if (taskZone.GetComponent<TaskZone>().taskItem.GetComponent<TaskItem>().disposable)
            {
                taskZone.GetComponent<TaskZone>().taskItem.tag = "Untagged";
            }

            if (taskZone.GetComponent<TaskZone>().taskItem.GetComponent<Item>() && !startCupFillingAnim)
            {
                taskZone.GetComponent<TaskZone>().taskItem.GetComponent<Item>().anim.SetBool("Filling", true);
                taskZone.GetComponent<TaskZone>().taskItem.GetComponent<Item>().filledCup = true;
                startCupFillingAnim = true;
            }

            if (completeTaskId != "")
            {
                foreach (Quest item in QuestManager.main.quests)
                {
                    if (item.id == completeTaskId && !item.complete)
                    {
                        item.complete = true;
                        QuestMarker.main.target = nextMarkerPoint;
                        break;
                    }
                }

                foreach (QuestItem item in FindObjectsOfType<QuestItem>())
                {
                    if (item.idQuest == completeTaskId)
                    {
                        item.CloseQuest();
                        break;
                    }
                }

                QuestManager.main.TaskSetup();
            }
        }

        if (itemPlaced && timer >= taskDelay && audioSource != null)
        {
            audioSource.Stop();

            if (hideZoneAfterTask)
            {
                taskZone.SetActive(false);
            }            
        }
    }
}
