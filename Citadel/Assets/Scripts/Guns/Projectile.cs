using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public GameObject damagePrefab;
    public LayerMask collisionLayers;
    public float maxExistanceTime = 3;
    public float damage = 100;
    public float sizeOfDamage = 1;

    [HideInInspector]
    public float hitForce;

    private void OnEnable()
    {
        StartCoroutine(ProjectileAlive());
    }

    public IEnumerator ProjectileAlive()
    {
        yield return new WaitForSeconds(maxExistanceTime);
        ActivateProjectile();
    }

    private void OnCollisionEnter(Collision col)
    {
        if (collisionLayers == (collisionLayers | (1 << col.gameObject.layer)))
        {
            Rigidbody colRigid = col.gameObject.GetComponent<Rigidbody>();
            if(colRigid)
            {
                colRigid.AddForce(transform.forward * GetComponent<Rigidbody>().velocity.magnitude);
            }
            ActivateProjectile(col.contacts[0].normal);
        }
    }

    public void ActivateProjectile(Vector3 hitNormal = new Vector3())
    {
        GameObject damageObject = Instantiate(damagePrefab, transform.position, Quaternion.identity);
        damageObject.tag = "PlayerProjectile";
        damageObject.GetComponent<ProjectileDamage>().incomingVector = -hitNormal;
        damageObject.GetComponent<ProjectileDamage>().damage = damage;
        damageObject.GetComponent<ProjectileDamage>().damageMask = collisionLayers;
        damageObject.transform.localScale = Vector3.one * sizeOfDamage;
        Destroy(gameObject);
    }
}
