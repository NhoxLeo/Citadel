using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameVars : MonoBehaviour
{
    public float musicVolumeScale = 0.5f, sfxVolumeScale = 0.5f;
    public AudioManager audioManager;

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
        else if (instance != this)
        {
            Destroy(instance.gameObject);
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
            if(musicVolumeScale != SlidersUI.instance.musicSlider.value)
            {
                musicVolumeScale = SlidersUI.instance.musicSlider.value;

                //PlayerVars.instance.audioSource.volume = gameMusicVolumeScale;
            }

            if (sfxVolumeScale != SlidersUI.instance.sfxSlider.value)
            {
                sfxVolumeScale = SlidersUI.instance.sfxSlider.value;
            }
        }
    }

    public void OnValidate()
    {
        //UnityEditor.EditorPrefs.SetBool("DeveloperMode", false);
    }
}
