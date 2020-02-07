using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameVars : MonoBehaviour
{
    public float musicVolumeScale = 0.5f, sfxVolumeScale = 0.5f;
    public AudioManager audioManager;
    public bool isPaused;
    public bool firstTimeSettings = true;

    [HideInInspector]
    public static GameVars instance; //Singleton

    /// <summary>
    /// Define Singleton
    /// </summary>
    private void Awake()
    {
        if (instance == null)
        {
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
            if (!firstTimeSettings)
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
                firstTimeSettings = false;
            }
        }
    }

    private void OnLevelWasLoaded(int level)
    {
        firstTimeSettings = true;
    }
}
