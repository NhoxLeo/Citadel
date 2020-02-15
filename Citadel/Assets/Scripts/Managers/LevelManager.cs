using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "New Level", menuName = "Levels")]
public class LevelManager : ScriptableObject
{
    [Header("Level Settings")]
    public string levelName;
    public string unitySceneName;
    public Sprite levelPreview;
}
