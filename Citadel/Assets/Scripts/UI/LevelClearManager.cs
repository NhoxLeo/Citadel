using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelClearManager : MonoBehaviour
{
    public Text levelTitle;
    public Text levelTime;
    public Text levelKills;
    public Text levelDamage;
    public Image background;
    public FadeToBlack fadeToBlack;

    public float minExitDelay = 5;

    private bool canExitMenu;

    // Start is called before the first frame update
    void Start()
    {
        if(GameVars.instance)
        {
            levelTitle.text = GameVars.instance.currentLevelManager.levelName;

            int minutes = Mathf.FloorToInt(GameVars.instance.totalTimeSpent / 60F);
            int seconds = Mathf.FloorToInt(GameVars.instance.totalTimeSpent - minutes * 60);
            string niceTime = string.Format("{0:0}:{1:00}", minutes, seconds);

            levelTime.text = "" + niceTime;
            levelKills.text = "" + GameVars.instance.totalEnemiesKilled;
            levelDamage.text = "" + GameVars.instance.totalDamageTaken;

            if(GameVars.instance.currentLevelManager.levelPreview)
            {
                background.sprite = GameVars.instance.currentLevelManager.levelPreview;
            }

            GameVars.instance.wasLevelBeaten = false;
        }

        StartCoroutine(AllowInput());
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.anyKeyDown)
        {
            if(canExitMenu)
            {
                if (GameVars.instance)
                {
                    if ((GameVars.instance.GetMatchingSceneIndexByName(GameVars.instance.currentLevelManager.unitySceneName) + 1) < GameVars.instance.levels.Count)
                    {
                        GameVars.instance.saveManager.levelsUnlockStatus[GameVars.instance.levels[GameVars.instance.GetMatchingSceneIndexByName(GameVars.instance.currentLevelManager.unitySceneName) + 1].levelUnlockIndex] = true;
                        GameVars.instance.saveManager.UpdateSaveData();

                        fadeToBlack.FadeToLevel(GameVars.instance.levels[GameVars.instance.GetMatchingSceneIndexByName(GameVars.instance.currentLevelManager.unitySceneName) + 1].unitySceneName);
                    }
                    else
                    {
                        fadeToBlack.FadeToLevel("Main Menu");
                    }
                }
                else
                {
                    fadeToBlack.FadeToLevel("Main Menu");
                }
            }
        }
    }

    public IEnumerator AllowInput()
    {
        yield return new WaitForSeconds(minExitDelay);
        canExitMenu = true;
    }
}
