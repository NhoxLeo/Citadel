using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Breakable : MonoBehaviour
{
    [Header("Breakable Settings")]
    public DestroyType destroyType = DestroyType.Force;
    [ConditionalHide("isForce", true)]
    public float MAX_FORCE; //Max Force that can be applied to an object before it breaks  
    public float TOTAL_HEALTH; //Total health this object has based on forces
    public float destructionForceMultiplier; //Additional force applied to gibs when an object is destroyed
    public bool doesDropItem = false; //Check if this breakable contains anything
    [ConditionalHide("doesDropItem", true)]
    public bool pickOneItemToSpawnRandomly; //Randomly chooses an item prefab to spawn
    [ConditionalHide("doesDropItem", true)]
    public float chanceOfSpawningItem = 100; //Change an item will be dropped

    [Header("Breakable References")]
    [Space(10)]
    public AudioClip breakSound; //Sound played on break
    public AudioClip hitSound; //Sound played on hit but not break
    public GameObject breakEffect; //Particle spawned on break

    [Header("Breakable Drops")]
    [Space(10)]
    public GameObject[] objectGibs; //Array of object gibs spawned when object is broken
    public List<GameObject> objectContents; //What the object contains and will spawn after desctruction

    [Header("Breakable Events")]
    [Space(10)]
    public List<UnityEvent> breakEvents;

    // Private & Stored variables
    [HideInInspector]
    public bool isForce;
    public enum DestroyType { Force, Health }

    private float impactVolumeToPlay = 1;
    private bool hasBroken;


    private void OnValidate()
    {
        if (destroyType == DestroyType.Force && !isForce)
        {
            isForce = true;
        }
        else if (destroyType != DestroyType.Force && isForce)
        {
            isForce = false;
        }
    }

   /// <summary>
    /// Detects an impact and determines if an object is broken
    /// </summary>
    /// <param name="col"></param>
    void OnCollisionEnter(Collision col)
    {
        Vector3 impactForce = col.impulse / Time.fixedDeltaTime;
        Impact(impactForce);
    }

    public void Impact(Vector3 impactForce, float healthDamage = 0)
    {
        if (destroyType == DestroyType.Force)
        {
            if (MAX_FORCE == 0)
            {
                ObjectBreak(impactForce);
            }
            else if (impactForce.magnitude >= MAX_FORCE)
            {
                ObjectBreak(impactForce);
            }
            else
            {
                PlayerCollideAudio(impactForce);
            }
        }
        else
        {
            TOTAL_HEALTH -= healthDamage;
            if(TOTAL_HEALTH <= 0)
            {
                ObjectBreak(impactForce);
            }
            else
            {
                PlayerCollideAudio(impactForce);
            }
        }
    }

    /// <summary>
    /// Takes Damage From RayCast Hits
    /// </summary>
    /// <param name="forceTaken"></param>
    public void OnRayCastEnter(Vector3 forceTaken)
    {
        Impact(forceTaken);
    }

    /// <summary>
    /// Spawns Gibs and stored Items when object is broken
    /// </summary>
    /// <param name="impactForce"></param>
    public void ObjectBreak(Vector3 impactForce)
    {
        if (!hasBroken)
        {
            hasBroken = true;
            gameObject.GetComponent<Rigidbody>().isKinematic = true; //Disable Object collisions to prevent clipping for gibs & items
                                                                     // Play the break sound
            if (GameVars.instance.audioManager)
            {
                GameVars.instance.audioManager.PlaySFX(breakSound, 1f, transform.position, null, 0, 1);
            }
            // Play the break effect
            if (breakEffect)
            {
                Instantiate(breakEffect, transform.position, Quaternion.identity);
            }
            //Spawn all given gibs within an object
            if (objectGibs.Length > 0)
            {
                foreach (GameObject gib in objectGibs)
                {
                    GameObject objectGib = Instantiate(gib, RandomPointInBounds(GetComponent<Collider>().bounds), Quaternion.Euler(Random.Range(0.0f, 360.0f), Random.Range(0.0f, 360.0f), Random.Range(0.0f, 360.0f)));
                    objectGib.GetComponent<Rigidbody>().AddForce(impactForce * destructionForceMultiplier);
                }
            }

            if (!pickOneItemToSpawnRandomly)
            {
                //Spawn all given contents within an object
                if (objectContents.Count > 0)
                {
                    foreach (GameObject item in objectContents)
                    {
                        GameObject objectItem = Instantiate(item, GetComponent<MeshRenderer>().bounds.center, transform.rotation);
                    }
                }
            }
            else
            {
                SingleRandomSpawn();
            }

            // Perform all break events
            foreach (UnityEvent evt in breakEvents)
            {
                evt.Invoke();
            }

            //Delete Self
            Destroy(gameObject);
        }
    }

    public Vector3 RandomPointInBounds(Bounds bounds)
    {
        return new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            Random.Range(bounds.min.y, bounds.max.y),
            Random.Range(bounds.min.z, bounds.max.z)
        );
    }


    /// <summary>
    /// Spawns a random item from the objectContents list
    /// </summary>
    void SingleRandomSpawn()
    {
        int randomIndex = Random.Range(0, objectContents.Count);
        float probabilityIndex = Random.Range(0, 100.0f);
        if (probabilityIndex >= (100.0f - chanceOfSpawningItem))
        {
            GameObject objectItem = Instantiate(objectContents[randomIndex], GetComponent<MeshRenderer>().bounds.center, transform.rotation);
        }
    }

    /// <summary>
    /// Plays specified clip on specified audioSource
    /// </summary>
    /// <param name="col"></param>
    private void PlayerCollideAudio(Vector3 impactForce)
    {
        impactVolumeToPlay = ScaleVolumeToForce((impactForce).magnitude, 5000);
        //Debug.Log("Impact Volume: " + impactVolumeToPlay);

        if (Time.timeSinceLevelLoad > 2)
        {
            GameVars.instance.audioManager.PlaySFX(hitSound, impactVolumeToPlay, transform.position, null, 0, 1);
        }
    }

    /// <summary>
    /// Scales a volume based on a force
    /// </summary>
    /// <param name="force"></param>
    /// <param name="maxForce"></param>
    /// <returns></returns>
    /// <summary>
    /// Scales a volume based on a force
    /// </summary>
    /// <param name="force"></param>
    /// <param name="maxForce"></param>
    /// <returns></returns>
    private float ScaleVolumeToForce(float force, float maxForce)
    {
        //Debug.Log(gameObject.name + " Impacted With With A Force Of " + force);
        float impactVolume = 0.0f;

        if (force > 0.05)
        {
            impactVolume = (force / maxForce);

            if (impactVolume > 1.0)
            {
                impactVolume = 1.0f;
            }

            if (impactVolume < 0.0)
            {
                impactVolume = 0.0f;
            }
        }

        return impactVolume;
    }
}
