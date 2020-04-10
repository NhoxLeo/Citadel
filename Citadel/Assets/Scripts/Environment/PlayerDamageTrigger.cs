using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VHS;

public class PlayerDamageTrigger : MonoBehaviour
{
    // Public Properties
    [Header("General Variables")]
    [Tooltip("How much damage the player takes when they hit the trigger.")]
    public float damageAmount = 10;
    [Tooltip("How long it takes for the trigger to reset before it can damage the player again.")]
    public float resetTime = 2f;
    public bool doesDamageEnemies = false;

    // Private Properties
    private bool hasBeenTriggered = false;

    // OnTriggerStay()
    private void OnTriggerStay(Collider other)
    {
        // IF the other object is the player...
        if (other.gameObject.layer == 8 && !hasBeenTriggered)
        {
            hasBeenTriggered = true;
            InteractionController.instance.TakeDamage(damageAmount);
            StartCoroutine(ResetTrigger());
        }

        if(doesDamageEnemies && other.transform.gameObject.layer == 12)
        {
            AIController controller = other.transform.parent.gameObject.GetComponent<AIController>();

            if(controller)
            {
                controller.TakeDamage(damageAmount);
            }
        }
    }

    // ResetTrigger()
    private IEnumerator ResetTrigger()
    {
        yield return new WaitForSeconds(resetTime);
        hasBeenTriggered = false;
    }
}
