using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gib : MonoBehaviour 
{

    [Header("Gib Settings")]
    public bool stayUpRight = false;
    public bool destroyGib = true;
    public Vector3 spawnRotation;
    [ConditionalHide("destroyGib", true)]
    public int gibExistenceTime = 5;
    [ConditionalHide("destroyGib", true)]
    public GameObject destroyParticle;

	// Use this for initialization
	void Start ()
    {
        if (destroyGib)
        {
            Invoke("gibDestroy", gibExistenceTime);
        }
	}
	
	// Update is called once per frame
	void Update ()
    {
		if(stayUpRight)
        {
            transform.rotation = Quaternion.Euler(spawnRotation);
        }
	}

    /// <summary>
    /// After a specified number of seconds, destory self and spawn a particle if defined
    /// </summary>
    void gibDestroy()
    {
        if(destroyParticle)
        {
            GameObject gibDestroyParticle = Instantiate(destroyParticle, transform.position, Quaternion.Euler(spawnRotation));
        }
        Destroy(gameObject);
    }
}
