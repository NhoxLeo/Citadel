using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class FadeToBlack : MonoBehaviour
{
    public Animator anim;
    private string sceneName;

    //call to activate fade trigger
    public void FadeToLevel(string levelName)
    {
        sceneName = levelName;
        anim.SetTrigger("FadeOut");
    }
    //Once the fade is complete we load the next level
    public void FadeComplete()
    {
        SceneManager.LoadScene(sceneName);
    }
}
