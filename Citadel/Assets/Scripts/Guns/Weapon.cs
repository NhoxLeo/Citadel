using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapons")]
public class Weapon : ScriptableObject
{
    [Header("Weapon Information")]
    public string weaponName = "DefaultWeapon";
    public WeaponType weaponType = WeaponType.Melee;

    [Header("Weapon References")]
    [Space(10)]
    public AudioClip attackSound;
    [ConditionalHide("isMelee", true, true)]
    public AudioClip reloadSound;
    [ConditionalHide("isMelee", true)]
    public AudioClip missSound;
    public AudioClip equipSound;
    [ConditionalHide("isProjectile", true)]
    public GameObject projectilePrefab;

    [Header("Weapon Settings")]
    [Space(10)]
    public float attackDamage = 10;
    [ConditionalHide("isRanged", true)]
    public float attackRange = 100;
    [ConditionalHide("isMelee", true)]
    public float attackRadius = 2;
    public float attackForce = 100;
    [ConditionalHide("isMelee", true, true)]
    public bool doesNeedReload = true;
    [ConditionalHide("isMelee", true, true)]
    public bool reloadOnShot = false;
    [ConditionalHide("doesNeedReload", true)]
    public int roundsPerClip = 8;
    [ConditionalHide("isMelee", true, true)]
    public int totalAmmoOnInitialPickup = 20;
    [ConditionalHide("isMelee", true, true)]
    public int totalAmmoOnAmmoPickup = 20;
    public int totalRoundsPerShot = 1;
    [ConditionalHide("isMultiShot", true)]
    public float spreadMaxDivation = 0;
    [ConditionalHide("isProjectile", true)]
    public float instantiationDistance = 0;

    [Header("Animation Settings")]
    public float attackStateLength = 0.2f;
    [ConditionalHide("isMelee", true, true)]
    public float reloadStateLength = 2;
    [ConditionalHide("isMelee", true, true)]
    public float shellEjectionDelay = 0;
    public float attackHitDelay = 0;

    [HideInInspector]
    public bool isMelee, isProjectile, isRanged;
    [HideInInspector]
    public bool isMultiShot;
    public enum WeaponType { Melee, Ranged, Projectile}

    /// <summary>
    /// Conditional Hide Inspector Tools
    /// </summary>
    private void OnValidate()
    {
        if (weaponType == WeaponType.Melee && !isMelee)
        {
            isMelee = true;
            isProjectile = false;
            isRanged = false;
            doesNeedReload = false;
        }
        else if (weaponType != WeaponType.Melee && isMelee)
        {
            isMelee = false;
        }

        if(weaponType == WeaponType.Projectile && !isProjectile)
        {
            isProjectile = true;
            isMelee = false;
            isRanged = false;
        }
        else if (weaponType != WeaponType.Projectile && isProjectile)
        {
            isProjectile = false;
        }

        if (weaponType == WeaponType.Ranged && !isRanged)
        {
            isProjectile = false;
            isMelee = false;
            isRanged = true;
        }
        else if (weaponType != WeaponType.Ranged && isRanged)
        {
            isRanged = false;
        }

        if (totalRoundsPerShot > 1 && !isMultiShot)
        {
            isMultiShot = true;
        }
        else if(totalRoundsPerShot <= 1 && isMultiShot)
        {
            isMultiShot = false;
            spreadMaxDivation = 0;
        }
    }
}
