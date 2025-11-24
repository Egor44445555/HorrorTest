using UnityEngine;
using UnityEngine.AI;
using TMPro;

public class Buyer : MonoBehaviour
{
    [SerializeField] Transform[] pathPoints;
    [SerializeField] Transform leavingPoint;
    [SerializeField] float stoppingDistance = 0.5f;
    [SerializeField] SkinnedMeshRenderer skinnedMesh;
    [SerializeField] Material idleMaterial;
    [SerializeField] Material smileMaterial;

    Animator npcAnimator;
    NavMeshAgent agent;
    int currentPathIndex = 0;
    float basedSpeed = 2f;
    bool bought = false;
    bool isWaiting = false;
    bool startLeaving = false;
    UIManager uIManager;

    void Start()
    {
        uIManager = UIManager.main;
        npcAnimator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        
        if (agent != null)
        {
            basedSpeed = agent.speed;
            agent.stoppingDistance = stoppingDistance;
        }

        if (agent == null || pathPoints.Length == 0)
        {
            return;
        }

        MoveToNextPoint();
    }

    void Update()
    {
        if (UIManager.main.gamePause) return;

        if (isWaiting) return;

        if (agent.isActiveAndEnabled && 
            agent.isOnNavMesh &&
            !agent.pathPending &&
            agent.remainingDistance <= agent.stoppingDistance)
        {
            HandlePointReached();
        }

        if (startLeaving)
        {
            float distanceToLeaving = Vector3.Distance(transform.position, leavingPoint.position);  

            if (distanceToLeaving <= 2f)
            {
                Destroy(gameObject);
            }
        }
    }

    void MoveToNextPoint()
    {
        if (currentPathIndex < pathPoints.Length)
        {
            agent.SetDestination(pathPoints[currentPathIndex].position);
            
            if (npcAnimator != null)
            {
                npcAnimator.SetBool("Walk", true);
            }
        }
    }

    void HandlePointReached()
    {
        currentPathIndex++;

        if (currentPathIndex < pathPoints.Length)
        {
            MoveToNextPoint();
        }
        else
        {
            StartWaiting();
        }
    }

    void StartWaiting()
    {
        isWaiting = true;
        
        if (npcAnimator != null)
        {
            npcAnimator.SetBool("Walk", false);
        }

        if (skinnedMesh != null)
        {
            skinnedMesh.material = smileMaterial;
        }

        if (QuestManager.main != null)
        {
            QuestManager.main.TaskSetup();
        }        
    }

    void CompletePurchase(float _cost)
    {
        if (bought) return;

        bought = true;

        if (uIManager != null)
        {
            uIManager.SetCost(_cost);
        }

        if (QuestManager.main != null)
        {            
            QuestManager.main.TaskClose("sell");
            QuestManager.main.TaskSetup();
        }
        
        StartLeaving();
    }

    void StartLeaving()
    {
        isWaiting = false;
        startLeaving = true;
        
        if (skinnedMesh != null)
        {
            skinnedMesh.material = idleMaterial;
        }

        currentPathIndex = 0;
        
        if (pathPoints.Length > 0)
        {
            agent.SetDestination(leavingPoint.position);
            
            if (npcAnimator != null)
            {
                npcAnimator.SetBool("Walk", true);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (isWaiting && !bought && LayerMask.LayerToName(other.gameObject.layer) == "Cup")
        {
            Item itemObj = other.gameObject.GetComponentInParent(typeof(Item)) as Item;

            if (itemObj != null && itemObj.GetComponent<TaskItem>().IsCompleted())
            {
                CompletePurchase(itemObj.GetCost());
                Destroy(itemObj.gameObject);
            }
        }
    }

    void OnDestroy()
    {
        skinnedMesh = null;
        idleMaterial = null;
        smileMaterial = null;
        npcAnimator = null;
        agent = null;
    }
}