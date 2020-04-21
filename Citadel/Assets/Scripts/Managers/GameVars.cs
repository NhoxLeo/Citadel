using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using VHS;

public class GameVars : MonoBehaviour
{
    public float musicVolumeScale = 0.5f, sfxVolumeScale = 0.5f;
    public AudioManager audioManager;
    public SaveDataManager saveManager;
    public DifficultyManager difficultyManager;
    public LevelManager currentLevelManager;
    public Level crawlManager;
    public List<LevelManager> levels;

    [HideInInspector]
    public bool isPaused;
    public bool wasLevelBeaten;
    public bool firstTimeSettingsSliders = true, firstTimeSettingsControls = true;
    public int currentCrawlLevel;
    public float currentCrawlHealth;
    public static GameVars instance; //Singleton

    //In level data
    [HideInInspector]
    public float totalTimeSpent;
    [HideInInspector]
    public int totalEnemiesKilled;
    [HideInInspector]
    public float totalDamageTaken;

    /// <summary>
    /// Define Singleton
    /// </summary>
    private void Awake()
    {
        if (instance == null)
        {
            OnLevelWasLoaded(SceneManager.GetActiveScene().buildIndex);
            instance = this;
        }
        else if (instance.gameObject != gameObject)
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);

        if (!audioManager)
        {
            Debug.LogError("No Audio Manager Found");
        }
        else
        {
            audioManager.gameVars = this;
        }
    }

    /// <summary>
    /// Check for setup variables
    /// </summary>
    private void Update()
    {
        if(SlidersUI.instance)
        {
            if (!firstTimeSettingsSliders)
            {
                if (musicVolumeScale != SlidersUI.instance.musicSlider.value)
                {
                    musicVolumeScale = SlidersUI.instance.musicSlider.value;
                }

                if (sfxVolumeScale != SlidersUI.instance.sfxSlider.value)
                {
                    sfxVolumeScale = SlidersUI.instance.sfxSlider.value;
                }
            }
            else
            {
                if (SlidersUI.instance.musicSlider.value != musicVolumeScale)
                {
                    SlidersUI.instance.musicSlider.value = musicVolumeScale;
                }

                if (SlidersUI.instance.sfxSlider.value != sfxVolumeScale)
                {
                    SlidersUI.instance.sfxSlider.value = sfxVolumeScale;
                }
                firstTimeSettingsSliders = false;
            }
        }

        if (ControlsUI.instance)
        {
            if (!firstTimeSettingsControls)
            {
                if (saveManager.SENSITIVITY != ControlsUI.instance.sensitivitySlider.value)
                {
                    saveManager.SENSITIVITY = ControlsUI.instance.sensitivitySlider.value;
                }
            }
            else
            {
                if (ControlsUI.instance.sensitivitySlider.value != saveManager.SENSITIVITY)
                {
                    ControlsUI.instance.sensitivitySlider.value = saveManager.SENSITIVITY;
                }
                firstTimeSettingsControls = false;
                if (InteractionController.instance != null)
                {
                    InteractionController.instance.transform.GetChild(0).GetComponent<CameraController>().UpdateSensitiviy();
                }
            }
        }

        #region Commands
        if (Input.GetKey(KeyCode.Slash))
        {
            string comboKeyCode = Input.inputString.ToLower();
            if (comboKeyCode == "r")
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
            else if (comboKeyCode == "g")
            {
                InteractionController.instance.maxHealth = 999999999;
                InteractionController.instance.playerHealth = 999999999;
            }
            else if (comboKeyCode == "d")
            {
                if (InteractionController.instance)
                {
                    InteractionController.instance.TakeDamage(999999999);
                }

            }
            else if (comboKeyCode == "l")
            {
                if ((GameVars.instance.GetMatchingSceneIndexByName(GameVars.instance.currentLevelManager.unitySceneName) + 1) < GameVars.instance.levels.Count)
                {
                    SceneManager.LoadScene(GameVars.instance.levels[GameVars.instance.GetMatchingSceneIndexByName(GameVars.instance.currentLevelManager.unitySceneName) + 1].unitySceneName);
                }
                else
                {
                    SceneManager.LoadScene("Main Menu");
                }
            }
        }
        #endregion
    }

    private void OnLevelWasLoaded(int level)
    {
        isPaused = false;
        firstTimeSettingsSliders = true;
        firstTimeSettingsControls = true;
        if (!wasLevelBeaten)
        {
            if (currentLevelManager)
            {
                ResetLevelData();
            }
            currentLevelManager = levels[GetMatchingSceneIndexByName(SceneManager.GetActiveScene().name)];
        }

        if(SceneManager.GetActiveScene().name == "Main Menu")
        {
            saveManager.ReadSaveData();
        }

        /*
        if (crawlManager)
        {
            crawlManager.levelIndex = currentCrawlLevel;
        }
        */
    }

    private void ResetLevelData()
    {
        totalDamageTaken = 0;
        totalEnemiesKilled = 0;
        totalTimeSpent = 0;
    }

    public int GetMatchingSceneIndexByName(string currentSceneName)
    {
        for(int i = 0; i < levels.Count; i++)
        {
            if(levels[i].unitySceneName == currentSceneName)
            {
                return i;
            }
        }
        return 0;
    }

    public void ChangeDifficulty(DifficultyManager.Difficulty newDifficulty)
    {
        difficultyManager.difficulty = newDifficulty;
    }
}