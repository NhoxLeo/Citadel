using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSelectManager : MonoBehaviour
{
    public List<LevelButton> levels;

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
    }
}
