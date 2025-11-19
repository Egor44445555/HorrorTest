using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager main;
    [SerializeField] GameObject monster;
    [SerializeField] TaskZone activeMonsterZone;
    [SerializeField] TaskZone safeZone;

    bool soundBeforeStart = false;
    bool startFinalQuest = false;

    void Awake()
    {
        if (main != null && main != this)
		{
			Destroy(gameObject);
			return;
		}
		
		main = this;
    }

    void Update()
    {
        if (activeMonsterZone && activeMonsterZone.taskItem)
        {
            startFinalQuest = true;
        }

        if (safeZone && safeZone.taskItem)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
            );
        }

        if (startFinalQuest)
        {
            monster.SetActive(true);

            if (!soundBeforeStart)
            {
                FindObjectOfType<PlayerController>().GetComponent<AudioSource>().Play();
                soundBeforeStart = true;
            }

            if (!FindObjectOfType<PlayerController>().GetComponent<AudioSource>().isPlaying)
            {
                monster.GetComponent<Monster>().startAttack = true;
            }
        }
    }

    void OnDestroy()
    {
        monster = null;
        activeMonsterZone = null;

        if (main == this)
        {
            main = null;
        }
    }
}
