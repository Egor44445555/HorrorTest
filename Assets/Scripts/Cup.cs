using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Cup : MonoBehaviour
{
    public GameObject taskZone;
    public GameObject[] components;
    public bool filledCup = false;

    [HideInInspector] public Animator anim;
    
    float timer = 0f;
    int maskNumber;
    LayerMask enemyMask;
    GameObject componentObject;
    List<GameObject> componentsList;

    void Start()
    {
        componentsList = new List<GameObject>(components);
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        timer += Time.deltaTime;
        GameObject obj = taskZone.GetComponent<TaskZone>().taskItem;

        if (filledCup)
        {
            GetComponent<TaskItem>().taskTarget = null;
        }

        if (taskZone.GetComponent<TaskZone>().taskInZone && obj != null && filledCup)
        {
            for (int i = 0; i < componentsList.Count; i++)
            {
                if (LayerMask.LayerToName(componentsList[i].layer) == LayerMask.LayerToName(obj.layer))
                {
                    GameObject newComponent = Instantiate(
                        componentsList[i],
                        componentsList[i].transform.position,
                        componentsList[i].transform.rotation
                    );

                    MeshFilter originalMeshFilter = obj.GetComponent<MeshFilter>();
                    MeshRenderer originalRenderer = obj.GetComponent<MeshRenderer>();

                    if (originalMeshFilter != null && originalRenderer != null)
                    {
                        MeshFilter newMeshFilter = newComponent.GetComponent<MeshFilter>();
                        MeshRenderer newRenderer = newComponent.GetComponent<MeshRenderer>();

                        if (newMeshFilter == null) newMeshFilter = newComponent.AddComponent<MeshFilter>();
                        if (newRenderer == null) newRenderer = newComponent.AddComponent<MeshRenderer>();

                        newMeshFilter.mesh = originalMeshFilter.mesh;
                        newRenderer.materials = originalRenderer.materials;
                    }

                    foreach (Quest item in QuestManager.main.quests)
                    {
                        if (item.id == LayerMask.LayerToName(obj.layer).ToLower())
                        {
                            item.complete = true;
                            break;
                        }
                    }

                    foreach (QuestItem item in FindObjectsOfType<QuestItem>())
                    {
                        if (item.idQuest == LayerMask.LayerToName(obj.layer).ToLower())
                        {
                            item.CloseQuest();
                            break;
                        }
                    }

                    if (componentsList.Count - 1 > i)
                    {
                        QuestManager.main.TaskSetup();
                    }
                    
                    newComponent.GetComponent<MeshRenderer>().enabled = true;
                    newComponent.transform.SetParent(transform, true);
                    Destroy(obj);
                    timer = 0f;
                    componentsList.RemoveAt(i);
                    break;
                }
            }
        }

        if (filledCup && components.Length > componentsList.Count && !PlayerController.main.isHolding)
        {
            GameObject component = null;

            foreach (GameObject componentItem in componentsList)
            {
                component = Array.Find(components, (item) => item.name == componentItem.name);
            }

            if (component)
            {
                Transform target = null;

                foreach (Quest item in QuestManager.main.quests)
                {
                    if (item.id == LayerMask.LayerToName(component.layer).ToLower())
                    {
                        target = item.target;
                        QuestMarker.main.target = item.target;
                        break;
                    }
                }
                
                foreach (QuestItem item in FindObjectsOfType<QuestItem>())
                {
                    if (item.idQuest == LayerMask.LayerToName(component.layer).ToLower() && target != null)
                    {
                        QuestMarker.main.target = target;
                        break;
                    }
                }
            }
        }

        if (componentsList.Count == 0 && filledCup)
        {
            GetComponent<TaskItem>().completed = true;
        }
    }
}
