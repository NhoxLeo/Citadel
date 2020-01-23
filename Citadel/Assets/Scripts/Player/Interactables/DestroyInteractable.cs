using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VHS
{    
    public class DestroyInteractable : InteractableBase
    {

        public override void OnInteract(Vector3 contactPoint, Transform playerGrip = null)
        {
            Destroy(gameObject);
        }
    }
}
