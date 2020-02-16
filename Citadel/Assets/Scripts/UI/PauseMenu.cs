using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public GameObject PauseMenuParent;
    public GameObject Main;
    public GameObject Settings;

    public GameObject FadeToBlack;
    public MenuState menuState = MenuState.Disabled;

    private AudioSource[] audioSources;

    public enum MenuState { Disabled, Main, ToMenu, Settings}

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Settings.SetActive(false);
        PauseMenuParent.SetActive(false);
        PauseMenuParent.transform.GetChild(1).GetChild(0).localScale = Vector3.one;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                Pause();
            }
            else
            {
                Resume();
            }
        }

        if(Cursor.lockState != CursorLockMode.Locked)
        {
            if(menuState == MenuState.Disabled)
            {
                Pause();
            }
        }
    }

    public void Resume()
    {
        GameVars.instance.isPaused = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        for (int i = 0; i < audioSources.Length; i++)
        {
            if (audioSources[i] != null)
            {
                if (audioSources[i].gameObject.transform.parent != gameObject.transform.parent)
                {
                    audioSources[i].Play();
                }
            }
        }

        UpdateState(MenuState.Disabled);
        Time.timeScale = 1;
    }

    public void Pause()
    {
        GameVars.instance.isPaused = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        audioSources = GameObject.FindObjectsOfType<AudioSource>();
        for (int i = 0; i < audioSources.Length; i++)
        {
            if (audioSources[i] != null)
            {
                if (audioSources[i].gameObject.transform.parent != gameObject.transform.parent)
                {
                    audioSources[i].Pause();
                }
            }
        }

        UpdateState(MenuState.Main);
        Time.timeScale = 0;
    }

    public void ToMain()
    {
        UpdateState(MenuState.Main);
    }

    public void ToSettings()
    {
        UpdateState(MenuState.Settings);
    }

    public void ToMainMenu()
    {
        UpdateState(MenuState.ToMenu);
    }

    public void Quit()
    {
        UpdateState(MenuState.ToMenu);
    }

    public void UpdateState(MenuState newState)
    {
        if(newState != menuState && menuState != MenuState.ToMenu)
        {
            menuState = newState;
            if (newState == MenuState.Main)
            {
                Settings.SetActive(false);

                PauseMenuParent.SetActive(true);
                Main.SetActive(true);
            }
            else if (newState == MenuState.Settings)
            {
                Main.SetActive(false);
                Settings.SetActive(true);
            }
            else if (newState == MenuState.ToMenu)
            {
                Cursor.visible = false;
                PauseMenuParent.SetActive(false);
                Time.timeScale = 1;
                FadeToBlack.SetActive(true);
                FadeToBlack.GetComponent<FadeToBlack>().FadeToLevel("Main Menu");
            }
            else if(newState == MenuState.Disabled)
            {
                PauseMenuParent.SetActive(false);
            }
        }
    }
}
