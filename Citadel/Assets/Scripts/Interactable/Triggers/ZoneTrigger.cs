using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ZoneTrigger : MonoBehaviour
{
    #region Variables & Inspector Options
    [Header("Setup Options")]
    #region Setup Options

    [Tooltip("Select if the zone should continue to react forever without being used up (Number Of Uses must be at least 1)")]
    [ConditionalHide("numberOfUses", true)]
    public bool isInfinite = false;

    [Tooltip("Number of uses before this zone will be used up and will no longer trigger")]
    public int numberOfUses = 1;
    #endregion

    [Space(10)]
    [Header("Compatible Objects")]
    #region Compatible Objects   
    public List<GameObject> compatiblesList;
    #endregion    

    [Space(10)]
    [Header("Event Calls")]
    #region Event Calls   
    public UnityEvent OnActivate;
    public UnityEvent OnReset;
    #endregion   

    #region Stored Data
    private bool activated;
    private GameObject triggeredObject;
    #endregion
    #endregion

    #region Listener Events
    private void OnTriggerEnter(Collider col)
    {
        if (!activated && numberOfUses > 0)
        {
            foreach (GameObject triggerObject in compatiblesList)
            {
                if (triggerObject == col.gameObject)
                {
                    triggeredObject = triggerObject;
                    activated = true;
                    if (OnActivate != null)
                    {
                        OnActivate.Invoke();
                    }
                    numberOfUses--;
                }
            }
        }
    }

    private void OnTriggerExit(Collider col)
    {
        if(activated)
        {
            if(col.gameObject == triggeredObject)
            {
                triggeredObject = null;
                if (OnReset != null)
                {
                    OnReset.Invoke();
                }
                activated = false;
            }
        }
    }
    #endregion
}
