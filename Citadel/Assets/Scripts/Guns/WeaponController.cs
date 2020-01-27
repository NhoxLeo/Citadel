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
    public GameObject weaponShell;
    [ConditionalHide("isMelee", true, true)]
    public GameObject reloadState;
    public WeaponState weaponState = WeaponState.Default;

    [HideInInspector]
    public enum WeaponState { Default, Attacking, Reloading}
    [HideInInspector]
    public bool isMelee;

    private int currentLoadedRounds;
    private int totalRounds;   

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
            totalRounds = weaponParams.totalAmmoOnPickup;
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
            if (weaponParams.weaponType == Weapon.WeaponType.Ranged)
            {
                if (currentLoadedRounds > 0)
                {
                    weaponState = WeaponState.Attacking;
                    StartCoroutine(Attack());
                }
                else if (totalRounds > 0)
                {
                    weaponState = WeaponState.Reloading;
                    StartCoroutine(Reload());
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
        int neededRounds = weaponParams.roundsPerClip - currentLoadedRounds;

        if(totalRounds > neededRounds)
        {
            totalRounds -= neededRounds;
            currentLoadedRounds += neededRounds;
        }
        else
        {
            currentLoadedRounds = totalRounds;
            totalRounds = 0;
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
            GameObject newShell = Instantiate(weaponShell, weaponShell.transform.position, weaponShell.transform.rotation);
            newShell.transform.parent = weaponShell.transform.parent;
            newShell.transform.localScale = weaponShell.transform.localScale;
            newShell.SetActive(true);
        }
        GameVars.instance.audioManager.PlaySFX(weaponParams.attackSound, 0.8f, transform.position, "WeaponSound");
        Damage();
        yield return new WaitForSeconds(weaponParams.attackStateLength);
        currentLoadedRounds--;
        if (weaponParams.reloadOnShot)
        {
            OnReload();
        }
        attackState.SetActive(false);
        defaultState.SetActive(true);
        weaponState = WeaponState.Default;
    }

    public void Damage()
    {
        RaycastHit hitInfo = new RaycastHit();
        bool rayCast = false;

        for (int i = 0; i < weaponParams.totalRoundsPerShot; i++)
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
                rayCast = Physics.SphereCast(Camera.main.transform.position, weaponParams.attackRadius, Camera.main.transform.forward, out hitInfo, weaponParams.attackRange, InteractionController.instance.bulletLayers);
            }

            if (rayCast)
            {
                if (hitInfo.rigidbody != null)
                {
                    hitInfo.rigidbody.AddForce(-hitInfo.normal * weaponParams.impactForce);
                }

                //If not an enemy
                if (hitInfo.transform.gameObject.layer == 0 && weaponParams.weaponType == Weapon.WeaponType.Ranged)
                {
                    GameObject bulletHole = Instantiate(InteractionController.instance.bulletHolePrefab, hitInfo.point - (-hitInfo.normal * 0.001f), Quaternion.LookRotation(-hitInfo.normal));
                    bulletHole.transform.parent = hitInfo.transform;
                    InteractionController.instance.bulletHoles.Add(bulletHole);
                }

                //Take Damange Here If Enemy
            }
        }
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
            if (currentLoadedRounds > 0 || totalRounds > 0)
            {
                GameVars.instance.audioManager.PlaySFX(weaponParams.equipSound, 0.8f, transform.position, "WeaponSound");
            }
        }
    }
}
