using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AICollisionCheck : MonoBehaviour
{
    public bool isPlayerColliding;
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
