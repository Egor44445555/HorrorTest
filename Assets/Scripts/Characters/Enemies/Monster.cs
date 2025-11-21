using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class Monster : MonoBehaviour
{
    public bool startAttack = false;
    [SerializeField] TaskZone activeZone;
    [SerializeField] SkinnedMeshRenderer[] skinnedMesh;


    [Header("Sounds")]
    [SerializeField] AudioSource roarSound;
    [SerializeField] AudioSource footstepSound;
    
    [Header("Target Settings")]
    [SerializeField] float targetRadius = 1f;
    [SerializeField] float minTargetRadius = 1f;

    Animator anim;
    NavMeshAgent agent;    
    float attackAnimationTimer = 0f;
    float maxAttackAnimationTime = 1.5f;
    Canvas canvas;
    bool attack = false;
    bool playMusic = false;
    Transform playerTransform;
    bool showModel = false;
    bool isAttacking = false;

    void Start()
    {
        playerTransform = PlayerController.main.transform;
        canvas = FindObjectOfType<Canvas>();
        anim = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();

        if (agent == null)
        {
            Debug.LogError("NavMeshAgent component is missing!", this);
            return;
        }
    }

    void Update()
    {
        if (!startAttack || playerTransform == null) return;

        if (skinnedMesh.Length > 0 && !showModel)
        {
            foreach(SkinnedMeshRenderer skin in skinnedMesh)
            {
                skin.enabled = true;
            }
            showModel = true;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);        
        bool shouldAttack = distanceToPlayer <= targetRadius && distanceToPlayer >= minTargetRadius;

        if (shouldAttack && !attack && !isAttacking)
        {
            StartAttack();
        }
        else if (!shouldAttack && attack)
        {
            StopAttack();
        }

        if (!attack && !isAttacking && distanceToPlayer > minTargetRadius)
        {
            if (agent != null && !agent.isStopped)
            {
                SetMoveToPoint(playerTransform.position);
            }
        }
        else if (distanceToPlayer <= minTargetRadius)
        {
            if (agent != null)
            {
                agent.isStopped = true;
            }
        }

        if (isAttacking)
        {
            attackAnimationTimer += Time.deltaTime;
            
            if (attackAnimationTimer >= maxAttackAnimationTime)
            {
                ForceStopAttack();
            }
                        
            if (anim != null && !IsAttackAnimationPlaying())
            {
                ForceStopAttack();
            }
        }

        UpdateAnimations();

        if (!playMusic)
        {
            GetComponent<AudioPlaylist>().playing = true;
            canvas.GetComponent<AudioSource>().Play();
            playMusic = true;
        }
    }

    void StartAttack()
    {
        attack = true;
        isAttacking = true;
        attackAnimationTimer = 0f;
        
        if (agent != null)
        {
            agent.isStopped = true;
        }

        int random = Random.Range(0, 100);
        bool secondAttack = random > 50;

        if (anim != null)
        {
            anim.SetBool("Attack", secondAttack);
            anim.SetBool("Attack2", !secondAttack);
        }
    }

    void StopAttack()
    {
        attack = false;
        
        if (agent != null)
        {
            agent.isStopped = false;
        }
    }

    void UpdateAnimations()
    {
        if (anim == null) return;

        anim.SetBool("Roar", false);        

        if (attack || isAttacking)
        {
            anim.SetBool("Walk", false);
        }
        else
        {
            bool isMoving = agent != null && agent.velocity.magnitude > 0.1f && !agent.isStopped;
            anim.SetBool("Walk", isMoving);            
            anim.SetBool("Attack", false);
            anim.SetBool("Attack2", false);
        }
    }

    public void FootStepSound()
    {
        if (footstepSound != null)
        {
            footstepSound.Play();
        }        
    }

    public void StartHunt()
    {
        if (anim == null)
        {
            anim = GetComponent<Animator>();            
        }

        if (roarSound != null)
        {
            roarSound.Play();
        }
        
        anim.SetBool("Roar", true);
    }

    public void StopAttackAnimation()
    {
        if (isAttacking)
        {
            ForceStopAttack();
        }
    }

    bool IsAttackAnimationPlaying()
    {
        if (anim == null) return false;

        AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
        
        bool isAttack1Playing = stateInfo.IsName("Attack") || anim.GetBool("Attack") && stateInfo.length > stateInfo.normalizedTime;
        bool isAttack2Playing = stateInfo.IsName("Attack2") || anim.GetBool("Attack2") && stateInfo.length > stateInfo.normalizedTime;

        return isAttack1Playing || isAttack2Playing;
    }

    void ForceStopAttack()
    {
        attack = false;
        isAttacking = false;
        attackAnimationTimer = 0f;
        
        if (agent != null)
        {
            agent.isStopped = false;
        }

        if (anim != null)
        {
            anim.SetBool("Attack", false);
            anim.SetBool("Attack2", false);
        }
    }

    public void SetMoveToPoint(Vector3 point)
    {
        if (agent != null && agent.isActiveAndEnabled)
        {
            agent.destination = point;
        }
    }
}