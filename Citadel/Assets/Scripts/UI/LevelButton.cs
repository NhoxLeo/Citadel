using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelButton : MonoBehaviour
{
    public Button levelButton;
    public Image levelImage;
    public Text levelName;

    public Sprite unlockedImage;
    public string unlockedName;
    public Sprite lockedImage;
    public string lockedName;

    public bool isUnlocked;

    public void UnlockLevel()
    {
        //Debug.Log("Unlocking");
        isUnlocked = true;
        levelImage.sprite = unlockedImage;
        levelName.text = unlockedName;
        levelButton.interactable = true;
    }

    public void LockLevel()
    {
        isUnlocked = false;
        levelButton.interactable = false;
        levelImage.sprite = lockedImage;
        levelName.text = lockedName;
    }
}
