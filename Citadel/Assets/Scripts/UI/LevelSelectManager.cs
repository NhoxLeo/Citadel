using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelectManager : MonoBehaviour
{
    public List<LevelButton> levels;
    public GameObject crawlOptions;
    public GameObject defaultBack;
    public Text crawlHS;

    public void UpdateLevels(List<bool> levelStatus)
    {
        for(int i = 0; i < levels.Count; i++)
        {
            if (levelStatus[i] == true)
            {
                //Debug.Log("Level " + i + " is Unlocked");
                levels[i].UnlockLevel();
            }
            else
            {
                //Debug.Log("Level " + i + " is Locked");
                levels[i].LockLevel();
            }
        }

        if(levels[levels.Count-1].isUnlocked)
        {
            defaultBack.SetActive(false);
            crawlOptions.SetActive(true);
            crawlHS.text = ":"+GameVars.instance.saveManager.crawlHighScore.ToString();
        }
        else
        {
            defaultBack.SetActive(true);
            crawlOptions.SetActive(false);
        }
    }
}
