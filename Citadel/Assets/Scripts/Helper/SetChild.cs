using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SetChild : MonoBehaviour
{
    [Space(10)]
    [Header("Compatible Objects")]
    #region Compatible Objects   
    public GameObject objectToAttach;
    public GameObject parentTransform;
    #endregion   

    private GameObject originalParent;

    public void AttachObject()
    {
        if(objectToAttach.transform.parent != null)
        {
            originalParent = objectToAttach.transform.parent.gameObject;
        }

        objectToAttach.transform.parent = parentTransform.transform;
    }

    public void RemoveObject()
    {
        if (originalParent == null)
        {
            objectToAttach.transform.parent = null;
        }
        else
        {
            objectToAttach.transform.parent = originalParent.transform;
        }
    }
}