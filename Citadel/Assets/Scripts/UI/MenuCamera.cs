using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Camera))]
public class MenuCamera : MonoBehaviour
{
    [SerializeField] private Transform[] cameraPositions;
    
    private const float lerpTime = 2;
    private float timer = 0;
    private bool startingLevelLoad;

    private Camera m_camera;

    private void Awake()
    {
        m_camera = GetComponent<Camera>();
    }

    private void Start()
    {
        ShowMenus(true);
        GameVars.instance.audioManager.PlayMusic("Menu");
    }

    private void Update()
    {
        if (m_camera.enabled)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        if (timer < lerpTime)
        {
            timer += Time.deltaTime;
            if(timer > lerpTime)
            {
                timer = lerpTime;
                transform.localRotation = Quaternion.identity;
                transform.localPosition = Vector3.zero;
            }
            else
            {
                float p = timer / lerpTime;
                p = p * p * (3 - 2 * p);
                transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.identity, p);
                transform.localPosition = Vector3.Lerp(transform.localPosition, Vector3.zero, p);
            }
        }
    }

   
    //////////////////////
    // BUTTON FUNCTIONS //
    //////////////////////

    public void Quit()
    {
        Application.Quit();
    }

    public void LoadScene(string scenename)
    {
        SceneManager.LoadScene(scenename, LoadSceneMode.Single);
    }

    private void ShowMenus(bool show)
    {
        foreach(Transform t in cameraPositions)
        {
            t.parent.gameObject.SetActive(show);
        }
    }

    public void GoToCameraPosition(int index)
    {        
        transform.SetParent(cameraPositions[index]);
        timer = 0;
    }

    public void UpdatePrefData()
    {
        GameVars.instance.saveManager.UpdatePrefData();
    }

    public void ChangeToEasy()
    {
        GameVars.instance.ChangeDifficulty(DifficultyManager.Difficulty.Easy);
    }

    public void ChangeToNormal()
    {
        GameVars.instance.ChangeDifficulty(DifficultyManager.Difficulty.Normal);
    }
    public void ChangeToHard()
    {
        GameVars.instance.ChangeDifficulty(DifficultyManager.Difficulty.Hard);
    }
}
