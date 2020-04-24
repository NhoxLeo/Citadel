using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectable : MonoBehaviour
{
    public int pickupAmount = 0;
    public AudioClip pickUpSound;

    private bool hasPickedUp;

    private void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.layer == 8 && !hasPickedUp) //Player
        {
            hasPickedUp = true;
            Pickup();
        }
    }

    private void Pickup()
    {
        GameVars.instance.audioManager.PlaySFX(pickUpSound, 0.5f, transform.position);
        Destroy(gameObject);
    }
}
