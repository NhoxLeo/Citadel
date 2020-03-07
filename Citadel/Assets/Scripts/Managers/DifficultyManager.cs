using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VHS;

public class DifficultyManager : MonoBehaviour
{
    [Header("Settings")]
    public Difficulty difficulty = Difficulty.Easy;
    [Range(0,1.0f)]
    public float ammoPickUpPercent = 0.10f;
    [Range(0, 1.0f)]
    public float regenSpeedPercent = 0.10f;
    [Range(0, 1.0f)]
    public float enemyDamagePercent = 0.10f;

    public enum Difficulty { Easy, Normal, Hard}

    public int GetDifficultyScaler()
    {
        if(difficulty == Difficulty.Easy)
        {
            return 1;
        }
        else if (difficulty == Difficulty.Hard)
        {
            return -1;
        }
        else
        {
            return 0;
        }
    }
}
