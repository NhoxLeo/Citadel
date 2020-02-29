using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ObjectiveBoolsTrigger : MonoBehaviour
{
    public string objectiveBools;
    public List<Animator> objectives;
    public UnityEvent OnComplete;

    private bool hasTriggered;

    // Update is called once per frame
    void Update()
    {
        if (!hasTriggered)
        {
            if (objectives != null & objectives.Count > 0)
            {
                bool isAllTrue = true;
                for (int i = 0; i < objectives.Count; i++)
                {
                    if (objectives[i].GetBool(objectiveBools) == false)
                    {
                        isAllTrue = false;
                    }
                }

                if (isAllTrue)
                {
                    hasTriggered = true;
                    OnComplete.Invoke();
                }
            }
        }
    }
}
