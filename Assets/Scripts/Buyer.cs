using UnityEngine;
using UnityEngine.AI;
using TMPro;

public class Buyer : MonoBehaviour
{
    public Transform[] pathPoints;
    public float orderWaitTime = 5f;
    public SkinnedMeshRenderer skinnedMesh;
    public Material idleMaterial;
    public Material smileMaterial;

    Animator npcAnimator;
    NavMeshAgent agent;
    AudioSource audioSource;
    int currentPathIndex = 0;
    float waitTimer = 0f;
    bool isActive = false;
    float basedSpeed = 2f;
    bool bought = false;
    bool startTasks = false;

    enum CustomerState
    {
        MovingToShop,
        MovingToCounter,
        WaitingForOrder,
        MovingToExit,
        Leaving
    }

    CustomerState currentState = CustomerState.MovingToShop;

    void Start()
    {
        npcAnimator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        audioSource = GetComponent<AudioSource>();
        basedSpeed = agent.speed;

        if (agent == null)
        {
            Debug.LogError("NavMeshAgent component is missing!", this);
            return;
        }

        if (pathPoints.Length < 3)
        {
            Debug.LogError("Not enough path points! Need at least 3: entrance, counter, exit", this);
            return;
        }

        isActive = true;
        MoveToNextPoint();

        if (npcAnimator != null)
        {
            npcAnimator.SetBool("Walk", true);
        }
    }

    void Update()
    {
        if (!isActive) return;

        switch (currentState)
        {
            case CustomerState.MovingToShop:
            case CustomerState.MovingToCounter:
            case CustomerState.MovingToExit:
                if (agent.isActiveAndEnabled && 
                    agent.isOnNavMesh &&
                    !agent.pathPending &&
                    agent.remainingDistance <= agent.stoppingDistance
                )
                {
                    ReachedPoint();
                }
                break;

            case CustomerState.WaitingForOrder:
                waitTimer += Time.deltaTime;
                agent.updateRotation = false;
                agent.speed = 0f;

                if (!startTasks)
                {
                    QuestManager.main.questList.gameObject.SetActive(true);
                    QuestManager.main.TaskSetup();
                    startTasks = true;
                }

                if (npcAnimator != null)
                {
                    npcAnimator.SetBool("Walk", false);
                }

                if (skinnedMesh != null)
                {
                    skinnedMesh.material = smileMaterial;
                }

                if (waitTimer >= orderWaitTime || bought)
                {
                    currentState = CustomerState.MovingToExit;
                    currentPathIndex = 2;

                    if (skinnedMesh != null)
                    {
                        skinnedMesh.material = idleMaterial;
                    }

                    if (agent.isActiveAndEnabled && agent.isOnNavMesh)
                    {
                        agent.SetDestination(pathPoints[currentPathIndex].position);
                    }

                    if (npcAnimator != null)
                    {
                        npcAnimator.SetBool("Walk", true);
                    }

                    agent.updateRotation = true;
                    agent.speed = basedSpeed;
                }
                break;

            case CustomerState.Leaving:
                if (agent.isActiveAndEnabled && agent.isOnNavMesh && !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
                {
                    Destroy(gameObject);
                }
                break;
        }
    }

    void MoveToNextPoint()
    {
        if (!isActive || !agent.isActiveAndEnabled || !agent.isOnNavMesh) return;

        if (currentPathIndex < pathPoints.Length)
        {
            agent.SetDestination(pathPoints[currentPathIndex].position);
        }
    }

    void ReachedPoint()
    {
        switch (currentState)
        {
            case CustomerState.MovingToShop:
                currentState = CustomerState.MovingToCounter;
                currentPathIndex = 1;

                if (agent.isActiveAndEnabled && agent.isOnNavMesh)
                {
                    agent.SetDestination(pathPoints[currentPathIndex].position);
                }
                break;

            case CustomerState.MovingToCounter:
                currentState = CustomerState.WaitingForOrder;
                waitTimer = 0f;

                if (npcAnimator != null)
                {
                    npcAnimator.SetBool("Walk", false);
                }
                break;

            case CustomerState.MovingToExit:
                currentState = CustomerState.Leaving;
                break;
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (LayerMask.LayerToName(other.gameObject.layer) == "Cup" && other.gameObject)
        {
            Cup cupObj = other.gameObject.GetComponentInParent(typeof(Cup)) as Cup;

            if (cupObj != null && cupObj.GetComponent<TaskItem>().completed)
            {
                float cost = 0f;
                bought = true;
                cost = 100;
                QuestManager.main.buyCost.GetComponent<TextMeshProUGUI>().text = cost.ToString() + "$";
                QuestManager.main.buying = true;
                QuestManager.main.buyingTarget = transform;
                QuestManager.main.TaskClose("sell");
                QuestManager.main.TaskSetup();
                audioSource.Play();
                Destroy(cupObj.gameObject);
            }
        }
    }
}