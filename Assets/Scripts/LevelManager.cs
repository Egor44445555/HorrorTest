using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager main;
    public GameObject monster;
    public TaskZone activeMonsterZone;

    bool soundBeforeStart = false;
    bool startFinalQuest = false;

    void Awake()
    {
        main = this;
    }

    void Update()
    {
        if (activeMonsterZone && activeMonsterZone.taskItem)
        {
            startFinalQuest = true;
        }

        if (startFinalQuest)
        {
            FindObjectOfType<PlayerController>().stuck = true;
            monster.SetActive(true);

            if (!soundBeforeStart)
            {
                FindObjectOfType<PlayerController>().GetComponent<AudioSource>().Play();
                soundBeforeStart = true;
            }

            if (!FindObjectOfType<PlayerController>().GetComponent<AudioSource>().isPlaying)
            {
                FindObjectOfType<PlayerController>().stuck = false;
                monster.GetComponent<Monster>().startAttack = true;
            }
        }
    }
}
