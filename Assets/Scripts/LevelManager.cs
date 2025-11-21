using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager main;
    [SerializeField] Monster monster;
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
        if (activeMonsterZone && activeMonsterZone.taskItem && !startFinalQuest)
        {
            monster.gameObject.SetActive(true);
            monster.StartHunt();

            if (!soundBeforeStart)
            {
                PlayerController.main.GetComponent<AudioSource>().Play();
                soundBeforeStart = true;
            }

            startFinalQuest = true;
        }

        if (startFinalQuest && !PlayerController.main.GetComponent<AudioSource>().isPlaying)
        {
            monster.startAttack = true;
        }

        if (safeZone && safeZone.taskItem)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
            );
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
