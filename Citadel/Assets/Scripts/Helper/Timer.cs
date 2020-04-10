using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Timer : MonoBehaviour
{
    // Public Properties
    [Header("Setup Options")]
    [Tooltip("The time (in seconds) it takes for the events to be fired.")]
    public float interval = 1;
    [Tooltip("Does the timer begin when the scene is loaded?")]
    public bool startOnSceneLoad = false;
    [ConditionalHide("startOnSceneLoad", true)]
    [Tooltip("The time (in seconds) when the timer starts after the scene has loaded.")]
    public float startDelay = 0;
    [Tooltip("Are the events fired repeatedly?")]
    public bool repeating = false;
    [ConditionalHide("repeating", true)]
    [Tooltip("Are the events infinitely repeating?")]
    public bool infinite = false;
    [ConditionalHide("repeating", true)]
    [Tooltip("How many times do the events repeat?")]
    public uint numberOfActivations = 1;

    [Space(10)]
    [Header("Event Calls")]
    public UnityEvent OnActivate;

    // Private Properties
    private uint firedCounter = 0;
    private bool disabled = true;

    // Start()
    void Start()
    {
        if (startOnSceneLoad)
        {
            disabled = false;
            if (startDelay > 0)
                StartCoroutine(PerformStartDelay());
            else
                StartCoroutine(TimerFunc());
        }
    }

    // ActivateTimer()
    public void ActivateTimer()
    {
        //Debug.Log("\tTimer.Activate() called!");
        if (disabled)
        {
            disabled = false;
            firedCounter = 0;
            StartCoroutine(TimerFunc());
        }
    }

    // DisableTimer()
    public void DisableTimer()
    {
        //Debug.Log("Timer.Disable() called!");
        disabled = true;
    }

    // PerformStartDelay()
    private IEnumerator PerformStartDelay()
    {
        yield return new WaitForSeconds(startDelay);
        StartCoroutine(TimerFunc());
    }

    // TimerFunc()
    private IEnumerator TimerFunc()
    {
        yield return new WaitForSeconds(interval);
        ++firedCounter;
        if (!disabled)
        {
            if (OnActivate != null)
                OnActivate.Invoke();
            if (repeating && (infinite || firedCounter < numberOfActivations))
                StartCoroutine(TimerFunc());
        }
    }
}
