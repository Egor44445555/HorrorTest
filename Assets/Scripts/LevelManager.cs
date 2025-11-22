using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager main;
    [SerializeField] Monster monster;
    [SerializeField] TaskZone activeMonsterZone;
    [SerializeField] TaskZone safeZone;
    [SerializeField] AudioSource chaseMusic;

    bool startFinalQuest = false;
    bool startPrepareMonster = false;
    float prepareMonsterTimer = 0f;
    float maxPrepareMonsterTime = 5f;
    bool isStartAttack = false;

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
        if (startPrepareMonster && prepareMonsterTimer < maxPrepareMonsterTime)
        {
            prepareMonsterTimer += Time.deltaTime;
        }

        if (activeMonsterZone != null && activeMonsterZone.taskItem && !startFinalQuest)
        {
            PlayerController.main.StartChase();
            startPrepareMonster = true;
            monster.gameObject.SetActive(true);
            monster.StartHunt();
            startFinalQuest = true;
        }

        if (startFinalQuest && prepareMonsterTimer >= maxPrepareMonsterTime && !isStartAttack)
        {
            monster.startAttack = true;

            if (chaseMusic != null)
            {
                chaseMusic.Play();
            }

            isStartAttack = true;
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
