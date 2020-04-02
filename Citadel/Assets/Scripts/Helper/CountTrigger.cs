using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CountTrigger: MonoBehaviour
{
    // Public Properties
    [Header("Debug")]
    public uint debugStartCount = 0;

    [Header("Count Events")]
    [Space(10)]
    public List<CountEvent> countEvents;

    // Private Properties
    public uint inputCount = 0;

    // Start is called before the first frame update
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
            if (countEvents[num].countNumber <= inputCount && !countEvents[num].eventsFired)
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
}

[System.Serializable]
public class CountEvent
{
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