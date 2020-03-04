using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VHS;

public class PlayerOrientationToggle : MonoBehaviour
{
    public Animator animator;
    public string boolParamToToggle;
    public bool doInvert;
    public string axis = "y";

    // Update is called once per frame
    void Update()
    {
        if(InteractionController.instance)
        {
            animator.SetBool(boolParamToToggle, GetPlayerOrientation());
        }
    }

    private bool GetPlayerOrientation()
    {
        Vector3 relativePoint = transform.InverseTransformPoint(InteractionController.instance.gameObject.transform.position);
        if (axis.ToLower() == "y")
        {
            if (relativePoint.y < 0.0)
            {
                if (doInvert)
                {
                    return false;
                }
                return true;
            }
        }
        else if (axis.ToLower() == "x")
        {
            if (relativePoint.x < 0.0)
            {
                if (doInvert)
                {
                    return false;
                }
                return true;
            }
        }
        else if (axis.ToLower() == "z")
        {
            if (relativePoint.z < 0.0)
            {
                if (doInvert)
                {
                    return false;
                }
                return true;
            }
        }

        if (doInvert)
        {
            return true;
        }
        return false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        if (axis.ToLower() == "y")
        {
            Gizmos.DrawLine(transform.position, transform.position + (transform.up));
        }
        else if (axis.ToLower() == "x")
        {
            Gizmos.DrawLine(transform.position, transform.position + (transform.right));
        }
        else if (axis.ToLower() == "z")
        {
            Gizmos.DrawLine(transform.position, transform.position + (transform.forward ));
        }
    }
}
