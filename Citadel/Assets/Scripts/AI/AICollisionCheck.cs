using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AICollisionCheck : MonoBehaviour
{
    [HideInInspector]
    public bool isPlayerColliding;
    [HideInInspector]
    public Vector3 attentionPoint;

    private void OnTriggerEnter(Collider col)
    {
        if(col.gameObject.CompareTag("WeaponSound"))
        {
            attentionPoint = col.gameObject.transform.position;
            //Debug.Log(gameObject.transform.parent.name + "heard a weapon sound from "+ col.gameObject.name);
        }
    }

    private void OnCollisionStay(Collision col)
    {
        if (col.gameObject.layer == 8)
        {
            isPlayerColliding = true;
        }
    }

    private void OnCollisionExit(Collision col)
    {
        if (col.gameObject.layer == 8)
        {
            isPlayerColliding = false;
        }
    }
}
