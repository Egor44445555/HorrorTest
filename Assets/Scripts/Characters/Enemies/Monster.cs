using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class Monster : MonoBehaviour
{
    [SerializeField] TaskZone activeZone;
    [SerializeField] SkinnedMeshRenderer[] skinnedMesh;
    public bool startAttack = false;

    Animator anim;
    NavMeshAgent agent;
    [SerializeField] float targetRadius = 1f;
    [SerializeField] float minTargetRadius = 1f;
    Canvas canvas;
    bool attack = false;
    bool playMusic = false;
    Transform playerTransform;
    bool showModel = false;

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
        if (startAttack && playerTransform != null)
        {
            if (skinnedMesh.Length > 0 && !showModel)
            {
                foreach(SkinnedMeshRenderer skin in skinnedMesh)
                {
                    skin.enabled = true;
                }
            }

            if (Vector3.Distance(transform.position, playerTransform.position) < targetRadius &&
                Vector3.Distance(transform.position, playerTransform.position) > minTargetRadius
            )
            {
                attack = true;
            }
            
            if (playerTransform != null && !attack)
            {
                SetMoveToPoint(playerTransform.position);
            }

            if (anim != null)
            {
                if (attack)
                {
                    if (agent != null)
                    {
                        agent.isStopped = true;
                    }

                    anim.SetBool("Walk", false);
                    anim.SetBool("Attack", true);
                }
                else
                {
                    anim.SetBool("Attack", false);
                    anim.SetBool("Walk", true);
                }
            }

            if (!playMusic)
            {
                GetComponent<AudioPlaylist>().playing = true;
                canvas.GetComponent<AudioSource>().Play();
                playMusic = true;
            }
        }
    }

    public void StopAttackAnimation()
    {
        attack = false;

        if (agent != null)
        {
            agent.isStopped = false;
        }
    }

    public void SetMoveToPoint(Vector3 point)
    {
        agent.destination = point;
    }
}