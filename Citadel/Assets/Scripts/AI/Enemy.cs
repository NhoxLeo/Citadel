using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "New Enemy", menuName = "Enemies")]
public class Enemy : ScriptableObject
{
    [Header("Enemy Information")]
    public string enemyName = "DefaultEnemy";
    public EnemyType enemyType = EnemyType.Melee;
    public EnemyBehavior enemyBehavior = EnemyBehavior.Stationary;

    [Header("AI References")]
    [Space(10)]
    public AudioClip damageSound;
    public AudioClip deathSound;
    public AudioClip idleSound;
    public AudioClip walkingSound;

    [Header("AI Settings")]
    [Space(10)]
    public float health = 100;
    public float walkSpeed = 10;
    public float fov = 90f;
    public float viewRange = 50f;
    [Range(0.0f, 20.0f)]
    public float playerRememberTime = 10;
    [Range(0.0f, 1f)]
    public float agressiveness = 1;

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
    [ConditionalHide("isProjectile", true, true)]
    public float attackRange = 100;
    [ConditionalHide("isMelee", true)]
    public float attackRadius = 2;
    public float attackPlayerRadius = 2.5f;
    public float attackForce = 100;
    public int totalRoundsPerShot = 1;
    public float spreadMaxDivation = 0;
    [ConditionalHide("isProjectile", true)]
    public float instantiationDistance = 0;
    public LayerMask damageMask;

    [Header("State Settings")]
    [ConditionalHide("isWander", true)]
    public float minWanderStateLength = 3f;
    [ConditionalHide("isWander", true)]
    public float maxWanderStateLength = 8f;
    [ConditionalHide("isWander", true)]
    public float minIdleStateTime = 2f;
    [ConditionalHide("isWander", true)]
    public float maxIdleStateTime = 5f;
    [ConditionalHide("isWander", true)]
    public float wanderRadius = 10;
    public float attackStateLength = 0.2f;
    public float attackDelayStateLength = 2;
    public float damageStateLength = 1;
    public float dyingStateLength = 2;

    [HideInInspector]
    public bool isMelee, isProjectile, isWander;
    public enum EnemyType { Melee, Ranged, Projectile, Friendly }
    public enum EnemyBehavior { Wander, Stationary}

    /// <summary>
    /// Conditional Hide Inspector Tools
    /// </summary>
    private void OnValidate()
    {
        if (enemyType == EnemyType.Melee && !isMelee)
        {
            isMelee = true;
            isProjectile = false;
        }
        else if (enemyType != EnemyType.Melee && isMelee)
        {
            isMelee = false;
        }

        if (enemyType == EnemyType.Projectile && !isProjectile)
        {
            isProjectile = true;
            isMelee = false;
        }
        else if(enemyType != EnemyType.Projectile && isProjectile)
        {
            isProjectile = false;
        }

        if(enemyBehavior == EnemyBehavior.Wander && !isWander)
        {
            isWander = true;
        }
        else if(enemyBehavior != EnemyBehavior.Wander && isWander)
        {
            isWander = false;
        }
    }
}
