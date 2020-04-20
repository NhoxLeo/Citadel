using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VHS;

public class HUDController : MonoBehaviour
{
    public Text playerClips;
    public Text playerRounds;
    public Text playerHealth;
    public float MAX_OPACTIY = 160;
    public float MIN_OPACTIY = 0;
    public float lerpDuration = 5;
    public float durationBeforeFade = 10;
    public List<HUDElement> hudElements; 

    [HideInInspector]
    public float currentOpacity = 160;
    [HideInInspector]
    public FadeState fadeState = FadeState.NONE;

    public enum FadeState { IN, OUT, NONE}

    private bool isLerpingFade = false;
    private bool hudLock = false;
    private Coroutine fadeCoroutine;
    
    // Start is called before the first frame update
    void Start()
    {
        DoUiFade(FadeState.OUT);
    }

    // Update is called once per frame
    void Update()
    {
        UpdateElementOpacity();
        playerHealth.text = InteractionController.instance.playerHealth.ToString();
    }

    public void UpdateWeaponInfo(string newClips, string newRounds)
    {
        playerClips.text = newClips;
        playerRounds.text = newRounds;
    }

    public void UpdateElementOpacity()
    {
        for (int i = 0; i < hudElements.Count; i++)
        {
            hudElements[i].elementOpacity = currentOpacity;
        }
    }

    public void DoUiFade(FadeState newFadeState, bool instant = false)
    {
        if (hudLock == false)
        {
            if (fadeState != FadeState.NONE)
            {
                if (fadeState != newFadeState)
                {
                    if (fadeCoroutine != null)
                    {
                        StopCoroutine(fadeCoroutine);
                        isLerpingFade = false;
                    }
                    StartFade(newFadeState, instant);
                }
            }
            else
            {
                StartFade(newFadeState, instant);
            }
        }
    }

    private void StartFade(FadeState newFadeState, bool instant = false)
    {
        fadeState = newFadeState;
        if (instant == false)
        {
            if (newFadeState == FadeState.IN)
            {
                fadeCoroutine = StartCoroutine(FadeUI(MAX_OPACTIY));
            }
            if (newFadeState == FadeState.OUT)
            {
                fadeCoroutine = StartCoroutine(FadeUI(MIN_OPACTIY));
            }
        }
        else
        {
            hudLock = true;
            if (newFadeState == FadeState.IN)
            {
                currentOpacity = MAX_OPACTIY;
            }
            if (newFadeState == FadeState.OUT)
            {
                currentOpacity = MIN_OPACTIY;
            }
        }
    }

    private IEnumerator StartFadeOut()
    {
        if (currentOpacity == MAX_OPACTIY)
        {
            yield return new WaitForSeconds(durationBeforeFade);
            if(fadeState == FadeState.NONE)
            {
                DoUiFade(FadeState.OUT, false);
            }
        }
    }

    private IEnumerator FadeUI(float targetOpacity)
    {
        isLerpingFade = true;
        float t = 0;

        while (currentOpacity != targetOpacity)
        {
            t += Time.deltaTime / lerpDuration;
            currentOpacity = Mathf.Lerp(currentOpacity, targetOpacity, t);
            yield return null;
        }
        isLerpingFade = false;
        fadeState = FadeState.NONE;
        StartCoroutine(StartFadeOut());
    }
}
