using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VHS;
public class Projectile : MonoBehaviour
{
    public ProjectileType projectileType = ProjectileType.Instant;
    public GameObject damagePrefab;
    public LayerMask collisionLayers;
    public float maxExistanceTime = 3;
    public float damage = 100;
    public float sizeOfDamage = 1;
    public bool lockRotation;
    private bool hasHitPlayer = false;

    [HideInInspector]
    public float hitForce;
    public enum ProjectileType { Instant, Timed}

    private Quaternion targetRotation;

    private void OnEnable()
    {
        targetRotation = transform.rotation;
        StartCoroutine(ProjectileAlive());
    }

    private void Update()
    {
        if(lockRotation)
        {
            transform.rotation = targetRotation;
        }
    }

    public IEnumerator ProjectileAlive()
    {
        yield return new WaitForSeconds(maxExistanceTime);
        ActivateProjectile(null);
    }

    private void OnCollisionEnter(Collision col)
    {
        if (projectileType == ProjectileType.Instant)
        {
            if (collisionLayers == (collisionLayers | (1 << col.gameObject.layer)))
            {
                Rigidbody colRigid = col.gameObject.GetComponent<Rigidbody>();
                if (colRigid)
                {
                    colRigid.AddForce(transform.forward * GetComponent<Rigidbody>().velocity.magnitude);
                }

                Breakable hitBreakable = col.gameObject.GetComponent<Breakable>();
                if (hitBreakable)
                {
                    hitBreakable.Impact(col.contacts[0].normal, damage);
                }
                ActivateProjectile(col, col.contacts[0].normal);
            }
        }

        if (projectileType == ProjectileType.Timed)
        {
            if(col.gameObject.layer == 12) //Ai
            {
                Rigidbody colRigid = col.gameObject.GetComponent<Rigidbody>();
                if (colRigid)
                {
                    colRigid.AddForce(transform.forward * GetComponent<Rigidbody>().velocity.magnitude);
                }
                ActivateProjectile(col, col.contacts[0].normal);
            }
        }
    }

    public void ActivateProjectile(Collision col, Vector3 hitNormal = new Vector3())
    {
        if (damagePrefab)
        {
            GameObject damageObject = Instantiate(damagePrefab, transform.position, Quaternion.identity);
            damageObject.tag = "PlayerProjectile";
            damageObject.GetComponent<ProjectileDamage>().incomingVector = -hitNormal;
            damageObject.GetComponent<ProjectileDamage>().damage = damage;
            //damageObject.GetComponent<ProjectileDamage>().damageMask = collisionLayers;
            damageObject.transform.localScale = Vector3.one * sizeOfDamage;
        }
        else
        {
            if (col != null)
            {
                Rigidbody colRigid = col.gameObject.GetComponent<Rigidbody>();
                if (col.gameObject.layer == 12)
                {
                    if (col.transform.parent)
                    {
                        AIController controller = col.transform.parent.gameObject.GetComponent<AIController>();
                        if (controller)
                        {
                            if (!controller.isDead)
                            {
                                controller.TakeDamage(damage, -hitNormal * 25);
                            }
                            else
                            {
                                col.transform.parent.GetComponent<Rigidbody>().AddForce(hitForce * -hitNormal);
                            }
                        }
                    }
                }
                else if (col.gameObject.layer == 8 && !hasHitPlayer)
                {
                    if (!InteractionController.instance.hasPlayerDied)
                    {
                        hasHitPlayer = true;
                        InteractionController.instance.TakeDamage(damage);
                    }
                }
                else
                {
                    Breakable hitBreakable = col.gameObject.GetComponent<Breakable>();
                    if (hitBreakable)
                    {
                        hitBreakable.Impact(-hitNormal * hitForce, damage);
                    }
                    if (colRigid)
                    {
                        colRigid.AddForce(hitForce * -hitNormal);
                    }
                }
            }
        }
        Destroy(gameObject);
    }
}
