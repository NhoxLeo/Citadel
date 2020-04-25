using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonParent : MonoBehaviour
{
    public GameObject objectToScale;
    public Vector3 buttonScaler;
    public float scaleDuration;
    public AudioClip clickSound;
    public AudioClip hoverSound;
    [Range(0, 1.0f)]
    public float buttonScaleTimeStep;

    [HideInInspector]
    public bool isCurrentlyActive;
    [HideInInspector]
    public bool isMouseActive;
    [HideInInspector]
    public bool playedHover;

    private bool newScaleReached;
    private Vector3 startingScale;

    // Use this for initialization
    void Start ()
    {
        if (!objectToScale)
        {            
            objectToScale = gameObject;
        }

        if(gameObject.transform.localScale == Vector3.zero)
        {
            startingScale = Vector3.one;
        }
        else
        {
            startingScale = gameObject.transform.localScale;
        }
    }

    // Update is called once per frame
    void Update ()
    {
        if (isCurrentlyActive)
        {
            if (!newScaleReached && (objectToScale.transform.localScale != buttonScaler))
            {
                if (!playedHover)
                {
                    playedHover = true;
                    if(GameVars.instance.audioManager)
                    {
                        GameVars.instance.audioManager.PlaySFX(hoverSound, 0.5f, Camera.main.ScreenToWorldPoint(Vector3.zero, Camera.MonoOrStereoscopicEye.Mono));
                    }
                }
                newScaleReached = true;
                ActivateButton();
            }
        }
        else
        {
            playedHover = false;
            if (!newScaleReached && (objectToScale.transform.localScale != startingScale))
            {               
                newScaleReached = true;
                DeactivateButton();
            }
        }
	}

    public void ActivateButton()
    {
        Vector3 currentScale = objectToScale.transform.localScale;
        for(float t = 0; t < 1; t+=buttonScaleTimeStep)
        {
            objectToScale.transform.localScale = new Vector3(Mathf.Lerp(currentScale.x, buttonScaler.x, t), Mathf.Lerp(currentScale.y, buttonScaler.y, t), Mathf.Lerp(currentScale.z, buttonScaler.z, t));
            t += Time.deltaTime / scaleDuration;
        }
        newScaleReached = false;
    }

    public void DeactivateButton()
    {
        Vector3 currentScale = objectToScale.transform.localScale;
        for (float t = 0; t < 1; t += buttonScaleTimeStep)
        {
            objectToScale.transform.localScale = new Vector3(Mathf.Lerp(currentScale.x, startingScale.x, t), Mathf.Lerp(currentScale.y, startingScale.y, t), Mathf.Lerp(currentScale.z, startingScale.z, t));
            t += Time.deltaTime / scaleDuration;
        }
        newScaleReached = false;
    }

    public void PointerEnter(BaseEventData eventData)
    {      
        isCurrentlyActive = true;
        isMouseActive = true;
    }

    public void PointerExit(BaseEventData eventData)
    {
        isCurrentlyActive = false;
        isMouseActive = false;
    }

    public void PointerUp(BaseEventData eventData)
    {
        if (GameVars.instance.audioManager)
        {
            GameVars.instance.audioManager.PlaySFX(clickSound, 0.5f, Camera.main.ScreenToWorldPoint(Vector3.zero, Camera.MonoOrStereoscopicEye.Mono));
        }
    }

    public void OnDisable()
    {
        isCurrentlyActive = false;
        isMouseActive = false;
        if (objectToScale)
        {
            objectToScale.transform.localScale = startingScale;
        }
    }
}
