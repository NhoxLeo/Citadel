using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VHS;

public class WeaponController : MonoBehaviour
{
    [Header("Weapon Information")]
    public Weapon weaponParams;
    public GameObject defaultState;
    public GameObject attackState;
    [ConditionalHide("isMelee", true, true)]
    public GameObject reloadState;
    public GameObject weaponShell;
    public GameObject attackParticle;
    public WeaponState weaponState = WeaponState.Default;

    [HideInInspector]
    public enum WeaponState { Default, Attacking, Reloading}
    [HideInInspector]
    public bool isMelee;
    [HideInInspector]
    public int totalRounds;

    private int currentLoadedRounds;
  

    /// <summary>
    /// Conditional Hide Inspector Tools
    /// </summary>
    private void OnValidate()
    {
        if (weaponParams)
        {
            if (weaponParams.weaponType == Weapon.WeaponType.Melee && !isMelee)
            {
                isMelee = true;
            }
            else if (weaponParams.weaponType != Weapon.WeaponType.Melee && isMelee)
            {
                isMelee = false;
            }
        }
        else
        {
            if(isMelee)
            {
                isMelee = false;
            }
        }
    }

    private void Start()
    {
        if (weaponParams)
        {
            totalRounds = weaponParams.totalAmmoOnInitialPickup;
            currentLoadedRounds = weaponParams.roundsPerClip;
        }
        else
        {
            Debug.LogError("ERROR | Weapon Parameters Not Found For " + gameObject.name);
        }
    }

    public void OnAttack()
    {
        if(weaponState == WeaponState.Default)
        {
            if (weaponParams.weaponType != Weapon.WeaponType.Melee)
            {
                if (currentLoadedRounds > 0 && weaponParams.doesNeedReload)
                {
                    weaponState = WeaponState.Attacking;
                    StartCoroutine(Attack());
                }
                else if (totalRounds > 0)
                {
                    if (weaponParams.doesNeedReload)
                    {
                        weaponState = WeaponState.Reloading;
                        StartCoroutine(Reload());
                    }
                    else if(totalRounds >= weaponParams.totalRoundsPerShot)
                    {
                        weaponState = WeaponState.Attacking;
                        StartCoroutine(Attack());
                    }
                }
            }
            else
            {
                weaponState = WeaponState.Attacking;
                StartCoroutine(Attack());
            }
        }
    }

    public void OnReload()
    {
        if (weaponState == WeaponState.Default)
        {
            if (weaponParams.weaponType == Weapon.WeaponType.Ranged)
            {
                if (weaponParams.doesNeedReload)
                {
                    if ((currentLoadedRounds != weaponParams.roundsPerClip) && (totalRounds > 0))
                    {
                        weaponState = WeaponState.Reloading;
                        StartCoroutine(Reload());
                    }
                }
                else
                {
                    FinishedReload();
                }
            }
        }

    }

    public void FinishedReload()
    {
        int neededRounds;
        if (weaponParams.doesNeedReload)
        {
            neededRounds = weaponParams.roundsPerClip - currentLoadedRounds;

            if (totalRounds > neededRounds)
            {
                totalRounds -= neededRounds;
                currentLoadedRounds += neededRounds;
            }
            else
            {
                currentLoadedRounds = totalRounds;
                totalRounds = 0;
            }
        }
        
        weaponState = WeaponState.Default;
    }

    public IEnumerator Reload()
    {
        defaultState.SetActive(false);
        reloadState.SetActive(true);
        GameVars.instance.audioManager.PlaySFX(weaponParams.reloadSound, 0.8f, transform.position, "WeaponSound");
        yield return new WaitForSeconds(weaponParams.reloadStateLength);
        reloadState.SetActive(false);
        defaultState.SetActive(true);
        FinishedReload();
    }

    public IEnumerator Attack()
    {
        defaultState.SetActive(false);
        attackState.SetActive(true);
        if (weaponShell)
        {
            StartCoroutine(EjectShell());
        }

        if (weaponParams.weaponType != Weapon.WeaponType.Melee)
        {
            GameVars.instance.audioManager.PlaySFX(weaponParams.attackSound, 0.8f, transform.position, "WeaponSound");
        }
        if (weaponParams.doesNeedReload)
        {
            currentLoadedRounds -= weaponParams.totalRoundsPerShot;
        }
        else
        {
            totalRounds -= weaponParams.totalRoundsPerShot;
        }
        if(attackParticle)
        {
            Instantiate(attackParticle, transform.position, Quaternion.identity);
        }
        Damage();
        yield return new WaitForSeconds(weaponParams.attackStateLength);
        if (weaponParams.reloadOnShot)
        {
            OnReload();
        }
        attackState.SetActive(false);
        defaultState.SetActive(true);
        weaponState = WeaponState.Default;
    }

    public IEnumerator EjectShell()
    {
        yield return new WaitForSeconds(weaponParams.shellEjectionDelay);
        GameObject newShell = Instantiate(weaponShell, weaponShell.transform.position, weaponShell.transform.rotation);
        newShell.transform.parent = weaponShell.transform.parent;
        newShell.transform.localScale = weaponShell.transform.localScale;
        newShell.SetActive(true);
    }

    public void Damage()
    {
        RaycastHit hitInfo = new RaycastHit();
        bool rayCast = false;

        for (int i = 0; i < weaponParams.totalRoundsPerShot; i++)
        {
            if (weaponParams.weaponType != Weapon.WeaponType.Projectile)
            {
                if (weaponParams.weaponType == Weapon.WeaponType.Ranged)
                {
                    Vector3 forwardVector = Vector3.forward;
                    float deviation = Random.Range(0f, weaponParams.spreadMaxDivation);
                    float angle = Random.Range(0f, 360f);
                    forwardVector = Quaternion.AngleAxis(deviation, Vector3.up) * forwardVector;
                    forwardVector = Quaternion.AngleAxis(angle, Vector3.forward) * forwardVector;
                    forwardVector = Camera.main.transform.rotation * forwardVector;

                    rayCast = Physics.Raycast(Camera.main.transform.position, forwardVector, out hitInfo, weaponParams.attackRange, InteractionController.instance.bulletLayers);
                }
                else
                {
                    rayCast = Physics.SphereCast(Camera.main.transform.position, weaponParams.attackRadius, Camera.main.transform.forward, out hitInfo, weaponParams.attackRange, InteractionController.instance.punchLayers);
                }

                if (rayCast)
                {
                    if (hitInfo.rigidbody != null)
                    {
                        hitInfo.rigidbody.AddForce(-hitInfo.normal * weaponParams.attackForce);
                    }

                    if (hitInfo.transform.gameObject.layer == 0 && weaponParams.weaponType == Weapon.WeaponType.Ranged)
                    {
                        GameObject bulletHole = Instantiate(InteractionController.instance.bulletHolePrefab, hitInfo.point - (-hitInfo.normal * 0.01f), Quaternion.LookRotation(-hitInfo.normal));
                        bulletHole.transform.parent = hitInfo.transform;
                        InteractionController.instance.bulletHoles.Add(bulletHole);
                    }

                    if (weaponParams.weaponType == Weapon.WeaponType.Melee)
                    {
                        GameVars.instance.audioManager.PlaySFX(weaponParams.attackSound, 0.8f, transform.position, "WeaponSound");
                    }

                    //Take Damange Here If Enemy
                    if(hitInfo.transform.gameObject.layer == 12)
                    {
                        if (hitInfo.transform.parent)
                        {
                            AIController controller = hitInfo.transform.parent.gameObject.GetComponent<AIController>();
                            if (controller)
                            {
                                controller.TakeDamage(weaponParams.attackDamage, -hitInfo.normal * weaponParams.attackForce*10);
                            }
                        }
                    }
                }
                else
                {
                    if (weaponParams.weaponType == Weapon.WeaponType.Melee)
                    {
                        if (weaponParams.missSound)
                        {
                            GameVars.instance.audioManager.PlaySFX(weaponParams.missSound, 0.8f, transform.position, "WeaponSound");
                        }
                    }
                }
            }
            else
            {
                StartCoroutine(FireProjectile());
            }
        }
    }

    public IEnumerator FireProjectile()
    {
        yield return new WaitForSeconds(weaponParams.shellEjectionDelay);
        GameObject projectile = Instantiate(weaponParams.projectilePrefab, transform.position, Camera.main.transform.rotation);
        projectile.GetComponent<Projectile>().damage = weaponParams.attackDamage;
        projectile.GetComponent<Projectile>().hitForce = weaponParams.attackForce;
        projectile.GetComponent<Rigidbody>().AddForce(Camera.main.transform.forward * weaponParams.attackForce);
    }

    private void OnDisable()
    {
        defaultState.SetActive(true);
        if (reloadState)
        {
            reloadState.SetActive(false);
        }
        if(attackState)
        {
            attackState.SetActive(false);
        }
        StopAllCoroutines();

        /*
        GameObject[] allCurrentSounds = GameObject.FindGameObjectsWithTag("WeaponSound");
        for (int i = 0; i < allCurrentSounds.Length; i++)
        {
            Destroy(allCurrentSounds[i]);
        }
        */

        weaponState = WeaponState.Default;
    }

    private void OnEnable()
    {
        if(weaponParams.equipSound)
        {
            if (weaponParams.doesNeedReload)
            {
                if (currentLoadedRounds > 0 || totalRounds > 0)
                {
                    GameVars.instance.audioManager.PlaySFX(weaponParams.equipSound, 0.8f, transform.position, "WeaponSound");
                }
            }
            else
            {
                if (totalRounds > weaponParams.totalRoundsPerShot)
                {
                    GameVars.instance.audioManager.PlaySFX(weaponParams.equipSound, 0.8f, transform.position, "WeaponSound");
                }
            }
        }
    }
}
