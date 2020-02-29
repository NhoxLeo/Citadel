using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VHS;

public class DeathPlane : MonoBehaviour
{
    private bool hasBeenTriggered;
    private void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.layer == 8 && !hasBeenTriggered)
        {
            hasBeenTriggered = true;
            InteractionController.instance.TakeDamage(InteractionController.instance.playerHealth);
        }
    }
}
