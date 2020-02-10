using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VHS;

public class ProjectileDamage : MonoBehaviour
{
    public AudioClip damageSound;
    public float maxExistanceTime = 2;
    public float maxDamageTime = 0.2f;
    public float damage = 100;
    public float explosionForce = 5000;
    public LayerMask damageMask;

    [HideInInspector]
    public List<GameObject> haveBeenHit;
    public Vector3 incomingVector;
    public bool hasHitPlayer;
    public bool doDamage = true;

    private void OnEnable()
    {
        haveBeenHit = new List<GameObject>();
        StartCoroutine(ProjectileAlive());
        StartCoroutine(ProjectileDoDamage());
    }

    private void OnTriggerEnter(Collider col)
    {
        if (damageMask == (damageMask | (1 << col.gameObject.layer)) && doDamage)
        {
            if (haveBeenHit.Contains(col.gameObject) == false)
            {
                haveBeenHit.Add(col.gameObject);
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
                                controller.TakeDamage(damage, incomingVector * explosionForce * 25);
                            }
                            else
                            {
                                col.transform.parent.GetComponent<Rigidbody>().AddExplosionForce(explosionForce * 10, transform.position, transform.localScale.magnitude);
                            }
                        }
                    }
                }
                else if (col.gameObject.layer == 8 && !hasHitPlayer)
                {
                    hasHitPlayer = true;
                    if (!InteractionController.instance.hasPlayerDied)
                    {
                        InteractionController.instance.TakeDamage(damage / 2);
                    }
                }
                else
                {
                    Breakable hitBreakable = col.gameObject.GetComponent<Breakable>();
                    if (hitBreakable)
                    {
                        hitBreakable.Impact(incomingVector * explosionForce, damage);
                    }
                    if (colRigid)
                    {
                        colRigid.AddExplosionForce(explosionForce, transform.position, transform.localScale.magnitude);
                    }
                }
            }
        }
    }

    public IEnumerator ProjectileAlive()
    {
        GameVars.instance.audioManager.PlaySFX(damageSound, 1f, transform.position);
        yield return new WaitForSeconds(maxExistanceTime);
        DestoryProjectile();
    }

    public IEnumerator ProjectileDoDamage()
    {
        yield return new WaitForSeconds(maxDamageTime);
        doDamage = false;
    }

    public void DestoryProjectile()
    {      
        Destroy(gameObject);
    }
}
