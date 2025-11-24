using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    [SerializeField] GameObject taskZone;
    [SerializeField] GameObject[] components;
    [SerializeField] float cost = 100f;

    [HideInInspector] public Animator anim;
    
    bool filledCup = false;
    List<GameObject> componentsList;
    TaskZone taskZoneComponent;
    TaskItem taskItemComponent;
    bool isProcessing = false;
    float lastCheckTime = 0f;
    float checkInterval = 0.2f;

    void Start()
    {
        componentsList = new List<GameObject>(components);
        anim = GetComponent<Animator>();
        taskZoneComponent = taskZone != null ? taskZone.GetComponent<TaskZone>() : null;
        taskItemComponent = GetComponent<TaskItem>();
    }

    void Update()
    {
        if (UIManager.main.gamePause) return;
        
        lastCheckTime += Time.deltaTime;

        if (lastCheckTime < checkInterval) return;

        lastCheckTime = 0f;

        if (isProcessing) return;

        ProcessTaskZoneInteraction();
        UpdateQuestMarker();
        CheckCompletion();
    }

    void ProcessTaskZoneInteraction()
    {
        if (taskZoneComponent == null || !taskZoneComponent.IsInTaskInZone() || !filledCup)
        {
            return;
        }

        GameObject taskObj = taskZoneComponent.GetTaskItem();

        if (taskObj == null)
        {
            return;
        }

        string taskLayerName = LayerMask.LayerToName(taskObj.layer);
        
        for (int i = componentsList.Count - 1; i >= 0; i--)
        {
            string componentLayerName = LayerMask.LayerToName(componentsList[i].layer);
            
            if (componentLayerName == taskLayerName)
            {
                isProcessing = true;
                ProcessComponentMatch(componentsList[i], taskObj, taskLayerName, i);
                isProcessing = false;
                return;
            }
        }
    }

    void ProcessComponentMatch(GameObject componentPrefab, GameObject taskObj, string layerName, int componentIndex)
    {
        GameObject newComponent = Instantiate(
            componentPrefab,
            componentPrefab.transform.position,
            componentPrefab.transform.rotation
        );

        CopyMeshAndMaterials(taskObj, newComponent);
        CompleteQuest(layerName.ToLower());

        newComponent.GetComponent<MeshRenderer>().enabled = true;
        newComponent.transform.SetParent(transform, true);
        
        Destroy(taskObj);
        componentsList.RemoveAt(componentIndex);

        if (componentsList.Count > 0)
        {
            QuestManager.main.TaskSetup();
        }
    }

    void CopyMeshAndMaterials(GameObject source, GameObject destination)
    {
        MeshFilter sourceMeshFilter = source.GetComponent<MeshFilter>();
        MeshRenderer sourceRenderer = source.GetComponent<MeshRenderer>();

        if (sourceMeshFilter == null || sourceRenderer == null)
        {
            return;
        }

        MeshFilter destMeshFilter = destination.GetComponent<MeshFilter>();
        MeshRenderer destRenderer = destination.GetComponent<MeshRenderer>();

        if (destMeshFilter == null) destMeshFilter = destination.AddComponent<MeshFilter>();
        if (destRenderer == null) destRenderer = destination.AddComponent<MeshRenderer>();

        destMeshFilter.mesh = sourceMeshFilter.mesh;
        destRenderer.materials = sourceRenderer.materials;
    }

    void CompleteQuest(string questId)
    {
        if (QuestManager.main == null) return;

        foreach (Quest quest in QuestManager.main.quests)
        {
            if (quest.id == questId && !quest.complete)
            {
                quest.complete = true;
                break;
            }
        }

        QuestManager.main.TaskClose(questId);
    }

    void UpdateQuestMarker()
    {
        if (!filledCup || componentsList.Count >= components.Length || PlayerController.main == null || PlayerController.main.isHolding)
        {
            return;
        }

        GameObject nextComponent = componentsList.Count > 0 ? componentsList[0] : null;
        
        if (nextComponent == null) return;

        string componentLayerName = LayerMask.LayerToName(nextComponent.layer).ToLower();
        Transform target = FindQuestTarget(componentLayerName);

        if (target != null && QuestMarker.main != null)
        {
            QuestMarker.main.SetTarget(target);
        }
    }

    Transform FindQuestTarget(string questId)
    {
        if (QuestManager.main == null || QuestManager.main.quests == null)
        {
            return null;
        }

        foreach (Quest quest in QuestManager.main.quests)
        {
            if (quest.id == questId && !quest.complete && quest.target != null)
            {
                return quest.target;
            }
        }

        return null;
    }

    void CheckCompletion()
    {
        if (componentsList.Count == 0 && filledCup && taskItemComponent != null)
        {
            taskItemComponent.SetCompleted();
            
            if (QuestMarker.main != null)
            {
                QuestMarker.main.ClearTarget();
            }
        }
    }

    public float GetCost()
    {
        return cost;
    }

    public void FillCup()
    {
        filledCup = true;
    }
}