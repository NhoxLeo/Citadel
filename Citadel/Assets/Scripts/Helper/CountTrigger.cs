using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CountTrigger: MonoBehaviour
{
    // Public Properties
    [Header("Settings")]
    [Tooltip("Do CountEvent instances fire when the EXACT amount of increments have happened? (Default behavior is <= increments)")]
    public bool exactCountFire = false;

    [Header("Debug")]
    public uint debugStartCount = 0;

    [Header("Count Events")]
    [Space(10)]
    public List<CountEvent> countEvents;

    // Private Properties
    private uint inputCount = 0;

    // Start()
    void Start()
    {
        // Setting Debug Options
        if (debugStartCount > 0)
        {
            inputCount = debugStartCount;
            FireCountEvents();
        }
    }

    // FireCountEvents()
    private void FireCountEvents()
    {
        for (int num = 0; num < countEvents.Count; num++)
        {
            uint countNumber = countEvents[num].countNumber;
            bool countCheck = exactCountFire ? (countNumber == inputCount) : (countNumber <= inputCount);
            if (countCheck && !countEvents[num].eventsFired)
            {
                StartCoroutine(CountEventCoroutine(num));
            }
        }
    }

    // CountEventCoroutine()
    private IEnumerator CountEventCoroutine(int ceIndex)
    {
        yield return new WaitForSeconds(countEvents[ceIndex].eventDelay);
        countEvents[ceIndex].fireEvents.Invoke();
        countEvents[ceIndex].eventsFired = true;
    }

    // Increment()
    public void Increment()
    {
        inputCount++;
        FireCountEvents();
    }

    // Reset()
    public void Reset()
    {
        inputCount = 0;
        foreach (CountEvent cEvent in countEvents)
        {
            cEvent.resetEvents.Invoke();
        }
    }

    // ManuallySetCounter()
    public void ManuallySetCounter(int newCount)
    {
        inputCount = (uint)newCount;
        FireCountEvents();
    }
}

[System.Serializable]
public class CountEvent
{
    [Header("Count Event Properties")]
    [Space(10)]
    [Tooltip("The name of this CountEvent instance. [No functional use, just for Unity editor convenience.]")]
    public string name;
    [Tooltip("How many increments have to happen before the events are fired?")]
    public uint countNumber = 1;
    [Tooltip("The delay (in seconds) of when the fire events should be invoked.")]
    public float eventDelay = 0f;
    [Tooltip("The events that happen when the desired amount of increments occur.")]
    public UnityEvent fireEvents;
    [Tooltip("The events that happen when the trigger is reset.")]
    public UnityEvent resetEvents;
    [HideInInspector]
    public bool eventsFired;

}