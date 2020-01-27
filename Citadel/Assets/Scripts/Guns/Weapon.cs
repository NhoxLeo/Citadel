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
    public AudioClip equipSound;

    [Header("Weapon Settings")]
    [Space(10)]
    public float attackDamange = 10;
    public float attackRange = 100;
    [ConditionalHide("isMelee", true)]
    public float attackRadius = 2;
    public float impactForce = 100;
    [ConditionalHide("isMelee", true, true)]
    public bool doesNeedReload = true;
    [ConditionalHide("isMelee", true, true)]
    public bool reloadOnShot = false;
    [ConditionalHide("doesNeedReload", true)]
    public int roundsPerClip = 8;
    [ConditionalHide("isMelee", true, true)]
    public int totalAmmoOnPickup = 20;
    public int totalRoundsPerShot = 1;
    [ConditionalHide("isMultiShot", true)]
    public float spreadMaxDivation = 0;

    [Header("Animation Settings")]
    public float attackStateLength = 0.2f;
    [ConditionalHide("isMelee", true, true)]
    public float reloadStateLength = 2;

    [HideInInspector]
    public bool isMelee;
    [HideInInspector]
    public bool isMultiShot;
    public enum WeaponType { Melee, Ranged}

    /// <summary>
    /// Conditional Hide Inspector Tools
    /// </summary>
    private void OnValidate()
    {
        if (weaponType == WeaponType.Melee && !isMelee)
        {
            isMelee = true;
            doesNeedReload = false;
        }
        else if (weaponType != WeaponType.Melee && isMelee)
        {
            isMelee = false;
        }

        if(totalRoundsPerShot > 1 && !isMultiShot)
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
