using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VHS;

public class CheckpointField : MonoBehaviour
{
    public AudioClip weaponClearSound;
    private bool currentlyIn;
    private void OnTriggerEnter(Collider col)
    {
        if(col.gameObject.layer == 8 && currentlyIn == false) //Player
        {
            InteractionController.instance.ClearPlayerWeapons();
            GameVars.instance.audioManager.PlaySFX(weaponClearSound, 0.2f, transform.position);
            currentlyIn = true;
        }
    }

    private void OnTriggerExit(Collider col)
    {
        if (col.gameObject.layer == 8 && currentlyIn == true) //Player
        {
            currentlyIn = false;
        }
    }
}
