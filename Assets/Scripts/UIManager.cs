using UnityEngine;
using System;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager main;
    [SerializeField] GameObject pauseMenu;
    [SerializeField] GameObject interfaceObj;
    

    [Header("Death Player Settings")]
    [SerializeField] GameObject deathMenu;

    [HideInInspector] public bool gamePause = false;

    void Awake()
    {
        if (main == null)
        {
            main = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        gamePause = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (pauseMenu != null)
        {
            pauseMenu.SetActive(false);
        }
        
        UnpauseGame();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus && !gamePause)
        {
            #if !UNITY_EDITOR
                PauseGame();
            #endif
        }
    }

    void TogglePause()
    {
        if (gamePause)
        {
            UnpauseGame();
        }
        else
        {
            PauseGame();
        }
    }

    void PauseGame()
    {
        if (gamePause) return;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        gamePause = true;
        AudioListener.pause = true;

        if (pauseMenu != null)
        {
            pauseMenu.SetActive(true);
        }

        Time.timeScale = 0f;
    }

    void UnpauseGame()
    {
        if (!gamePause) return;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        AudioListener.pause = false;
        gamePause = false;

        if (pauseMenu != null)
        {
            pauseMenu.SetActive(false);
        }

        Time.timeScale = 1f;     
    }

    public void ResumeGame()
    {
        UnpauseGame();
    }
    
    public void Restart()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    
    void OnDestroy()
    {
        if (main == this)
        {
            main = null;
        }
    }
}