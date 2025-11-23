using UnityEngine;
using System;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager main;
    [SerializeField] GameObject pauseMenu;
    [SerializeField] GameObject buyCost;
    [SerializeField] AudioSource buySound;

    [HideInInspector] public bool gamePause = false;

    Animator buyCostAnimator;
    TextMeshProUGUI buyCostText;
    float cost = 0f;
    float showCostTimer = 0f;
    bool startShowCost = false;

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
        buyCostAnimator = buyCost.GetComponent<Animator>();
        buyCostText = buyCost.GetComponent<TextMeshProUGUI>();

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

        if (startShowCost)
        {
            showCostTimer += Time.deltaTime;
        }

        if (startShowCost && showCostTimer >= 1f)
        {
            HideCost();
        }
    }

    public void SetCost(float _cost)
    {
        startShowCost = true;
        buyCost.SetActive(true);
        buyCostAnimator.SetBool("Buy", true);
        cost += _cost;
        buyCostText.text = _cost.ToString() + "$";

        if (buySound != null)
        {
            buySound.Play();
        }
    }

    public void HideCost()
    {
        startShowCost = false;
        showCostTimer = 0f;
        buyCostAnimator.SetBool("Buy", false);
        buyCost.SetActive(false);
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