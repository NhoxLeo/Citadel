using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileDamage : MonoBehaviour
{
    public AudioClip damageSound;
    public float maxExistanceTime = 2;
    public float damage = 100;
    public float explosionForce = 5000;
    public LayerMask damageMask;

    [HideInInspector]
    public List<GameObject> haveBeenHit;

    private void OnEnable()
    {
        haveBeenHit = new List<GameObject>();
        StartCoroutine(ProjectileAlive());
    }

    private void OnTriggerEnter(Collider col)
    {
        if (damageMask == (damageMask | (1 << col.gameObject.layer)))
        {
            Rigidbody colRigid = col.gameObject.GetComponent<Rigidbody>();
            if (colRigid)
            {
                colRigid.AddExplosionForce(explosionForce, transform.position, transform.localScale.magnitude);
            }
        }
    }

    public IEnumerator ProjectileAlive()
    {
        GameVars.instance.audioManager.PlaySFX(damageSound, 1f, transform.position);
        yield return new WaitForSeconds(maxExistanceTime);
        DestoryProjectile();
    }

    public void DestoryProjectile()
    {      
        Destroy(gameObject);
    }
}
