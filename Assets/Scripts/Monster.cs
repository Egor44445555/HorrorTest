using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class Monster : MonoBehaviour
{
    public TaskZone activeZone;
    public SkinnedMeshRenderer skinnedMesh;
    public bool startAttack = false;

    Animator npcAnimator;
    NavMeshAgent agent;
    int currentPathIndex = 0;
    float waitTimer = 0f;
    float basedSpeed = 2f;
    Canvas canvas;
    bool playMusic = false;

    void Start()
    {
        canvas = FindObjectOfType<Canvas>();
        npcAnimator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        basedSpeed = agent.speed;

        if (agent == null)
        {
            Debug.LogError("NavMeshAgent component is missing!", this);
            return;
        }
    }

    void Update()
    {
        if (startAttack)
        {
            skinnedMesh.enabled = true;
            agent.SetDestination(FindObjectOfType<PlayerController>().transform.position);

            if (!playMusic)
            {
                GetComponent<AudioPlaylist>().playing = true;
                canvas.GetComponent<AudioSource>().Play();
                playMusic = true;
            }

            if (npcAnimator != null)
            {
                npcAnimator.SetBool("Walk", true);
            }
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Collider>().gameObject.CompareTag("Player"))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
            );
        } 
    }
}