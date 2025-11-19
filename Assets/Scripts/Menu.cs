using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Menu : MonoBehaviour
{
    public static Menu main;
    public GameObject menu;

    void Awake()
    {
        if (main != null && main != this)
		{
			Destroy(gameObject);
			return;
		}
		
		main = this;
    }

    void Start()
    {
        Time.timeScale = 1;
    }

    public void OpenMenu()
    {
        menu.SetActive(true);
        Time.timeScale = 0;
    }

    public void Return()
    {
        menu.SetActive(false);
        Time.timeScale = 1;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void Repeat()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
        );
    }

    public void Exit()
    {
        Application.Quit();
    }

    void OnDestroy()
    {
        menu = null;

        if (main == this)
        {
            main = null;
        }
    }
}
