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
    AudioSource audioSource;
    int currentPathIndex = 0;
    float basedSpeed = 2f;
    bool bought = false;
    bool isWaiting = false;
    bool startLeaving = false;
    QuestManager questManager;

    void Start()
    {
        questManager = QuestManager.main;
        npcAnimator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        audioSource = GetComponent<AudioSource>();
        
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
        if (isWaiting) return;

        if (agent.isActiveAndEnabled && 
            agent.isOnNavMesh &&
            !agent.pathPending &&
            agent.remainingDistance <= agent.stoppingDistance)
        {
            HandlePointReached();
        }

        if (startLeaving && agent.remainingDistance <= agent.stoppingDistance)
        {
            Destroy(gameObject);
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

        if (questManager != null)
        {
            questManager.questList.gameObject.SetActive(true);
            questManager.TaskSetup();
        }        
    }

    void CompletePurchase()
    {
        if (bought) return;

        bought = true;
        
        float cost = 100f;

        if (questManager != null)
        {
            questManager.buyCost.GetComponent<TextMeshProUGUI>().text = cost.ToString() + "$";
            questManager.buying = true;
            questManager.buyingTarget = transform;
            questManager.TaskClose("sell");
            questManager.TaskSetup();
        }        
        
        if (audioSource != null)
        {
            audioSource.Play();
        }

        StartLeaving();
    }

    void StartLeaving()
    {
        isWaiting = false;
        startLeaving = true;

        if (questManager != null && questManager.buyingTarget == transform)
        {
            questManager.buyingTarget = null;
        }
        
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

            if (itemObj != null && itemObj.GetComponent<TaskItem>().completed)
            {
                CompletePurchase();
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
        audioSource = null;
    }
}