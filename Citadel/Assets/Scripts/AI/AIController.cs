using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VHS;
using UnityEngine.AI;
using UnityEngine.Events;

public class AIController : MonoBehaviour
{
    #region Variables & Inspector Settings
    [Header("AI References")]
    public Enemy enemyParams;
    public FieldOfView fieldOfView;
    public NavMeshAgent navMeshAgent;

    [Header("AI States")]
    [Space(10)]
    public GameObject aliveBody;
    public GameObject deadBody;
    public GameObject idleState;
    public GameObject walkState;
    public GameObject attackState;
    public GameObject damageState;
    public GameObject dieState;
    public GameObject deadState;
    public GameObject attackParticle;

    [Header("AI Death Events")]
    [Space(10)]
    public List<UnityEvent> deathEvents;

    [Header("Debug")]
    [Space(10)]
    public AIState aiState = AIState.Idle;
    public List<AICollisionCheck> aiCollisionChecks;

    [HideInInspector]
    public GameObject currentTarget;
    [HideInInspector]
    public bool isDead;
    [HideInInspector]
    public float playerSpottedTimer;
    [HideInInspector]
    public enum AIState { Idle, Wander, Attack, Damage, Dying, Dead }

    #region Stored Data

    private GameObject currentState;
    private float currentAIHealth;
    private GameObject player;
    private bool currentlyInState;
    private bool playerSpotted;
    private Vector3 wanderCenteralPoint;
    
    private Vector3 launchVector;
    private Vector3 lastKnownPlayerLocation;
    private NavMeshHit navMeshHit;
    private Vector3 forwardVector;
    private Coroutine movingCoroutine;
    private Coroutine attackingCoroutine;
    private Coroutine damageCoroutine;
    private Coroutine attackTimerCoroutine;
    private Coroutine wanderCoroutine;
    private GameObject damageSoundObject;
    #endregion
    #endregion

    #region Unity Event Methods
    private void Awake()
    {
        if (!NavMesh.SamplePosition(transform.position, out navMeshHit, 4f, NavMesh.AllAreas))
        {
            gameObject.SetActive(false);
            Debug.LogError("[ERROR] - No Navmesh Found For AI " + gameObject.name);
        }
        else
        {
            transform.position = navMeshHit.position;
        }
    }

    /// <summary>
    /// Start is called before the first frame update
    /// </summary>
    void Start()
    {
        player = InteractionController.instance.gameObject.transform.parent.GetChild(1).gameObject;
        currentAIHealth = enemyParams.health;
        currentState = idleState;
        navMeshAgent.speed = enemyParams.walkSpeed;
        wanderCenteralPoint = transform.position;
    }

    /// <summary>
    /// Update is called once per frame
    /// </summary>
    void Update()
    {
        if (!isDead)
        {          
            CheckForPlayer();
            if (!playerSpotted)
            {
                if (currentTarget == player)
                {
                    currentTarget = null;
                }

                if (!currentlyInState && playerSpottedTimer == 0)
                {
                    if (enemyParams.enemyBehavior == Enemy.EnemyBehavior.Stationary)
                    {
                        SwitchState(AIState.Idle);
                    }
                    else if (enemyParams.enemyBehavior == Enemy.EnemyBehavior.Wander)
                    {
                        SwitchState(AIState.Wander);
                    }
                }
                else
                {
                    if (aiState == AIState.Attack)
                    {
                        if (playerSpottedTimer == 0)
                        {
                            currentlyInState = false;
                            if (attackTimerCoroutine != null)
                            {
                                StopCoroutine(attackTimerCoroutine);
                            }
                            attackTimerCoroutine = null;
                        }
                        else
                        {
                            if (lastKnownPlayerLocation != transform.position)
                            {
                                transform.rotation = Quaternion.LookRotation(lastKnownPlayerLocation - transform.position, Vector3.up);
                                transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
                            }
                            else
                            {
                                if (movingCoroutine == null)
                                {
                                    movingCoroutine = StartCoroutine(LookAround());
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                if (!currentTarget)
                {
                    currentTarget = player;
                    if (enemyParams.equipSound)
                    {
                        GameVars.instance.audioManager.PlaySFX(enemyParams.equipSound, 0.8f, transform.position, "WeaponSound", 0, 0.5f);
                    }
                    SwitchState(AIState.Attack);
                }
            }
        }

        transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
    }

    /// <summary>
    /// Debug Gizmos
    /// </summary>
    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(transform.position, transform.position + forwardVector * 2);

        Gizmos.DrawSphere(navMeshHit.position, 0.2f);
    }
    #endregion

    #region State Methods
    /// <summary>
    /// Takes a new state and switches to it
    /// </summary>
    /// <param name="newState"></param>
    public void SwitchState(AIState newState)
    {
        aiState = newState;
        currentState.SetActive(false);

        if (newState == AIState.Idle)
        {
            currentState = idleState;
        }
        else if (newState == AIState.Wander)
        {
            if (wanderCoroutine == null)
            {
                wanderCoroutine = StartCoroutine(WanderState());
            }
        }
        else if (newState == AIState.Attack)
        {
            if (attackingCoroutine == null)
            {
                if (attackTimerCoroutine == null)
                {
                    attackTimerCoroutine = StartCoroutine(AttackStateTimer());
                }
                attackingCoroutine = StartCoroutine(AttackState());
            }
        }
        else if (newState == AIState.Damage)
        {
            if (damageCoroutine == null)
            {
                damageCoroutine = StartCoroutine(DamageState());
            }
        }
        else if (newState == AIState.Dying)
        {
            StopAllCoroutines();
            damageCoroutine = StartCoroutine(DieState());
        }
        else if (newState == AIState.Dead)
        {
            currentState = deadState;
            GameVars.instance.totalEnemiesKilled += 1;
            aliveBody.SetActive(false);
            deadBody.SetActive(true);
        }

        currentState.SetActive(true);
    }

    /// <summary>
    /// Handles the wandering of AI
    /// </summary>
    /// <returns></returns>
    public IEnumerator WanderState()
    {
        currentlyInState = true;

        Vector3 pointToWanderTo = GetRandomCirclePosition();
        movingCoroutine = StartCoroutine(GoToPosition(pointToWanderTo, "", false, movingCoroutine));
        float wanderStateLength = Random.Range(enemyParams.minWanderStateLength, enemyParams.maxWanderStateLength);
        yield return new WaitForSeconds(wanderStateLength);

        if (movingCoroutine != null)
        {
            StopCoroutine(movingCoroutine);
        }
        navMeshAgent.SetDestination(transform.position);
        navMeshAgent.velocity = Vector3.zero;

        movingCoroutine = StartCoroutine(LookAround());

        float idleStateLength = Random.Range(enemyParams.minIdleStateTime, enemyParams.maxIdleStateTime);
        yield return new WaitForSeconds(idleStateLength);

        if (movingCoroutine != null)
        {
            StopCoroutine(movingCoroutine);
        }
        navMeshAgent.SetDestination(transform.position);
        navMeshAgent.velocity = Vector3.zero;
        movingCoroutine = null;
        wanderCoroutine = null;

        SwitchState(AIState.Idle);
        currentlyInState = false;
    }

    /// <summary>
    /// Handles AI attacking
    /// </summary>
    /// <returns></returns>
    public IEnumerator AttackState()
    {
        if (currentTarget == player)
        {
            #region Reset State
            if (movingCoroutine != null)
            {
                StopCoroutine(movingCoroutine);
                navMeshAgent.SetDestination(transform.position);
                navMeshAgent.velocity = Vector3.zero;
                movingCoroutine = null;
            }

            if (wanderCoroutine != null)
            {
                StopCoroutine(wanderCoroutine);
                navMeshAgent.SetDestination(transform.position);
                navMeshAgent.velocity = Vector3.zero;
                wanderCoroutine = null;
            }
            #endregion

            currentlyInState = true;
            if (enemyParams.enemyType != Enemy.EnemyType.Melee) //Ranged & Projectile
            {
                EnemyAttack();
                yield return new WaitForSeconds(enemyParams.attackStateLength);
                UpdateSprite(idleState);
                if (enemyParams.reloadSound)
                {
                    GameVars.instance.audioManager.PlaySFX(enemyParams.reloadSound, 0.8f, transform.position, "WeaponSound", 0, 0.5f);
                }
                yield return new WaitForSeconds(enemyParams.attackDelayStateLength);
            }
            else //Melee
            {
                bool hasArrived = false;
                if (Vector3.Distance(transform.position, player.transform.position) < 2.5)
                {
                    EnemyAttack();
                    yield return new WaitForSeconds(enemyParams.attackStateLength);
                    UpdateSprite(idleState);
                    if (enemyParams.reloadSound)
                    {
                        GameVars.instance.audioManager.PlaySFX(enemyParams.reloadSound, 0.8f, transform.position, "WeaponSound", 0, 0.5f);
                    }
                    yield return new WaitForSeconds(enemyParams.attackDelayStateLength);
                }
                else
                {
                    while (hasArrived == false)
                    {
                        transform.rotation = Quaternion.LookRotation(player.transform.position - transform.position, Vector3.up);
                        transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);

                        if (CalculateNewPath(player.transform.position) == true)
                        {
                            navMeshAgent.SetDestination(player.transform.position);
                            if (Vector3.Distance(transform.position, player.transform.position) < 2.5)
                            {
                                //Debug.Log("Arrived Local");
                                navMeshAgent.SetDestination(transform.position);
                                lastKnownPlayerLocation = transform.position;
                                navMeshAgent.velocity = Vector3.zero;
                                hasArrived = true;
                                UpdateSprite(idleState);

                                EnemyAttack();
                                yield return new WaitForSeconds(enemyParams.attackStateLength);
                                UpdateSprite(idleState);
                                if (enemyParams.reloadSound)
                                {
                                    GameVars.instance.audioManager.PlaySFX(enemyParams.reloadSound, 0.8f, transform.position, "WeaponSound", 0, 0.5f);
                                }
                                yield return new WaitForSeconds(enemyParams.attackDelayStateLength);
                            }
                            if (!hasArrived)
                            {
                                UpdateSprite(walkState);
                            }
                        }
                        else
                        {
                            navMeshAgent.SetDestination(transform.position);
                            lastKnownPlayerLocation = transform.position;
                            navMeshAgent.velocity = Vector3.zero;
                            hasArrived = true;
                            UpdateSprite(idleState);
                        }
                        yield return null;
                    }

                    yield return new WaitUntil(() => (hasArrived == true));
                }
            }
        }

        attackingCoroutine = null;

        //After Attack Check if Player Is Still Visible
        if (currentTarget == player)
        {
            if (enemyParams.enemyBehavior == Enemy.EnemyBehavior.Stationary)
            {
                //Keep Shooting
                SwitchState(AIState.Attack);
            }
            else if (enemyParams.enemyBehavior == Enemy.EnemyBehavior.Wander)
            {
                if (Vector3.Distance(transform.position, player.transform.position) > 6)
                {
                    //Walk Closer Then Shoot
                    attackingCoroutine = StartCoroutine(GoToPosition(lastKnownPlayerLocation, "", false, attackingCoroutine));
                    yield return new WaitForSeconds(1 * enemyParams.agressiveness);
                    if (attackingCoroutine != null)
                    {
                        StopCoroutine(attackingCoroutine);
                    }
                    navMeshAgent.SetDestination(transform.position);
                    navMeshAgent.velocity = Vector3.zero;
                    attackingCoroutine = null;
                    SwitchState(AIState.Attack);
                }
                else
                {
                    SwitchState(AIState.Attack);
                }
            }
        }
        else
        {
            // Go To Where The Player Was
            if (transform.position != lastKnownPlayerLocation)
            {
                if (movingCoroutine == null)
                {
                    movingCoroutine = StartCoroutine(GoToPosition(lastKnownPlayerLocation, "", false, movingCoroutine));
                    if (enemyParams.agressiveness < 1)
                    {
                        yield return new WaitForSeconds(10 * enemyParams.agressiveness);
                        StopCoroutine(movingCoroutine);
                        navMeshAgent.SetDestination(transform.position);
                        navMeshAgent.velocity = Vector3.zero;
                        movingCoroutine = null;
                    }
                }
            }
            else
            {
                if (movingCoroutine == null)
                {
                    //Debug.Log("Look Around");
                    movingCoroutine = StartCoroutine(LookAround());
                }
            }
        }
    }

    /// <summary>
    /// Handles the damage indicators
    /// </summary>
    /// <returns></returns>
    public IEnumerator DamageState()
    {
        currentlyInState = true;
        UpdateSprite(damageState);
        yield return new WaitForSeconds(enemyParams.damageStateLength);
        UpdateSprite(idleState);
        damageCoroutine = null;
        currentlyInState = false;
        yield return new WaitForSeconds(enemyParams.attackDelayStateLength / 2);
        SwitchState(AIState.Attack);
    }

    /// <summary>
    /// Handles AI Death
    /// </summary>
    /// <returns></returns>
    public IEnumerator DieState()
    {
        currentlyInState = true;
        UpdateSprite(dieState);
        Rigidbody rigidbody = gameObject.AddComponent<Rigidbody>();
        rigidbody.mass = 100;
        rigidbody.angularDrag = 5;
        navMeshAgent.enabled = false;
        rigidbody.AddForce(launchVector);
        if (enemyParams.deathSound)
        {
            if(damageSoundObject != null)
            {
                damageSoundObject.GetComponent<AudioSource>().Stop();
            }
            damageSoundObject = GameVars.instance.audioManager.PlaySFX(enemyParams.deathSound, 0.8f, transform.position, "DamageSound", 0, 0.5f);
        }
        yield return new WaitForSeconds(enemyParams.dyingStateLength);
        SwitchState(AIState.Dead);
        currentlyInState = false;
    }
    #endregion

    #region State Supplimentary Methods
    /// <summary>
    /// Checks if player is within line of sight or touching
    /// </summary>
    public void CheckForPlayer()
    {
        if (InteractionController.instance.hasPlayerDied == false)
        {
            playerSpotted = false;
            if (fieldOfView)
            {
                if (fieldOfView.visibleTargets.Count > 0)
                {
                    playerSpottedTimer = enemyParams.playerRememberTime;
                    if (NavMesh.SamplePosition(player.transform.position, out navMeshHit, 4f, NavMesh.AllAreas))
                    {
                        lastKnownPlayerLocation = navMeshHit.position;
                    }
                    transform.rotation = Quaternion.LookRotation(player.transform.position - transform.position, Vector3.up);
                    transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
                    playerSpotted = true;
                }
            }

            if (aiCollisionChecks != null)
            {
                if (aiCollisionChecks.Count > 0)
                {
                    foreach (AICollisionCheck collisionCheck in aiCollisionChecks)
                    {
                        if (collisionCheck.isPlayerColliding)
                        {
                            navMeshAgent.SetDestination(transform.position);
                            navMeshAgent.velocity = Vector3.zero;
                            playerSpottedTimer = enemyParams.playerRememberTime;
                            if (NavMesh.SamplePosition(player.transform.position, out navMeshHit, 4f, NavMesh.AllAreas))
                            {
                                lastKnownPlayerLocation = navMeshHit.position;
                            }
                            transform.rotation = Quaternion.LookRotation(player.transform.position - transform.position, Vector3.up);
                            transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
                            playerSpotted = true;
                        }
                    }
                }
            }
        }
        else
        {
            playerSpotted = false;
            currentTarget = null;
        }
    }

    /// <summary>
    /// If the AI doesn't see the player, this timer will tick down
    /// </summary>
    /// <returns></returns>
    public IEnumerator AttackStateTimer()
    {
        if (playerSpottedTimer > 0)
        {
            yield return new WaitForSeconds(1);
            playerSpottedTimer -= 1;
            attackTimerCoroutine = StartCoroutine(AttackStateTimer());
        }
    }

    /// <summary>
    /// Looks around in circle for player
    /// </summary>
    /// <returns></returns>
    public IEnumerator LookAround()
    {
        UpdateSprite(idleState);
        int lookIncrement = 8;
        float turnDelay = 0.5f;
        for (int i = 0; i < 360; i += (360 / lookIncrement))
        {
            transform.Rotate(0, (360 / lookIncrement), 0);
            yield return new WaitForSeconds(turnDelay);
        }
        movingCoroutine = null;
    }

    /// <summary>
    /// Takes a damage value & an impact vector
    /// </summary>
    /// <param name="damageToTake"></param>
    /// <param name="launchVector"></param>
    public void TakeDamage(float damageToTake, Vector3 launchVector = new Vector3())
    {
        if (currentAIHealth > 0)
        {
            if (damageCoroutine == null)
            {
                navMeshAgent.SetDestination(transform.position);
                navMeshAgent.velocity = Vector3.zero;

                //StopAllCoroutines();
                //attackingCoroutine = null;
                //movingCoroutine = null;
                //wanderCoroutine = null;
                //attackTimerCoroutine = null;

                playerSpottedTimer = enemyParams.playerRememberTime;
                transform.rotation = Quaternion.LookRotation(player.transform.position - transform.position, Vector3.up);
                transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);

            }

            if (currentAIHealth > damageToTake)
            {
                if (enemyParams.damageSound && damageSoundObject == null)
                {
                    damageSoundObject = GameVars.instance.audioManager.PlaySFX(enemyParams.damageSound, 0.8f, transform.position, "DamageSound", 0, 0.5f);
                }
                SwitchState(AIState.Damage);
            }
            else if (currentAIHealth <= damageToTake)
            {
                this.launchVector = launchVector;
                isDead = true;
                SwitchState(AIState.Dying);

                // Executing the death events
                foreach (UnityEvent evt in deathEvents)
                {
                    evt.Invoke();
                }
            }

            currentAIHealth -= damageToTake;
        }
    }

    /// <summary>
    /// Handles the bullets & damage of the enemy attack
    /// </summary>
    public void EnemyAttack()
    {
        UpdateSprite(attackState);

        if (enemyParams.enemyType != Enemy.EnemyType.Melee)
        {
            GameVars.instance.audioManager.PlaySFX(enemyParams.attackSound, 0.8f, transform.position, "WeaponSound", 0, 0.5f);
        }
        if (attackParticle)
        {
            Instantiate(attackParticle, transform.position, Quaternion.identity);
        }

        RaycastHit hitInfo = new RaycastHit();
        bool rayCast = false;

        for (int i = 0; i < enemyParams.totalRoundsPerShot; i++)
        {
            if (enemyParams.enemyType != Enemy.EnemyType.Projectile)
            {
                if (enemyParams.enemyType == Enemy.EnemyType.Ranged)
                {
                    Vector3 deviation3D = Random.insideUnitCircle * enemyParams.spreadMaxDivation;
                    Quaternion rot = Quaternion.LookRotation(Vector3.forward * (((enemyParams.attackRange) - Vector3.Distance(transform.position, player.transform.position)) - 30) + deviation3D);
                    forwardVector = rot * (player.transform.position - transform.position);

                    rayCast = Physics.Raycast(transform.position, forwardVector, out hitInfo, enemyParams.attackRange, enemyParams.damageMask);
                }
                else
                {
                    rayCast = Physics.SphereCast(transform.position, enemyParams.attackRadius, (player.transform.position - transform.position), out hitInfo, enemyParams.attackRange, enemyParams.damageMask);
                }

                if (rayCast)
                {
                    if (hitInfo.rigidbody != null)
                    {
                        hitInfo.rigidbody.AddForce(-hitInfo.normal * enemyParams.attackForce);
                    }

                    Breakable hitBreakable = hitInfo.collider.gameObject.GetComponent<Breakable>();
                    if (hitBreakable)
                    {
                        hitBreakable.Impact(-hitInfo.normal * enemyParams.attackForce, enemyParams.attackDamage);
                    }

                    if (hitInfo.transform.gameObject.layer == 0 && enemyParams.enemyType == Enemy.EnemyType.Ranged)
                    {
                        GameObject bulletHole = Instantiate(InteractionController.instance.bulletHolePrefab, hitInfo.point - (-hitInfo.normal * 0.01f), Quaternion.LookRotation(-hitInfo.normal));
                        bulletHole.transform.parent = hitInfo.transform;
                        InteractionController.instance.bulletHoles.Add(bulletHole);
                    }

                    if (enemyParams.enemyType == Enemy.EnemyType.Melee)
                    {
                        GameVars.instance.audioManager.PlaySFX(enemyParams.attackSound, 0.8f, transform.position, "WeaponSound", 0, 0.5f);
                    }

                    //Take Damange Here If Enemy
                    if (hitInfo.transform.gameObject.layer == 12)
                    {
                        if (hitInfo.transform.parent)
                        {
                            AIController controller = hitInfo.transform.parent.gameObject.GetComponent<AIController>();
                            if (controller)
                            {
                                controller.TakeDamage(enemyParams.attackDamage, -hitInfo.normal * enemyParams.attackForce * 10);
                            }
                        }
                    }
                    else if (hitInfo.transform.gameObject.layer == 8) //Player Damage
                    {
                        if (hitInfo.transform.parent)
                        {
                            InteractionController.instance.TakeDamage(enemyParams.attackDamage);
                        }
                    }
                }
                else
                {
                    if (enemyParams.enemyType == Enemy.EnemyType.Melee)
                    {
                        if (enemyParams.missSound)
                        {
                            GameVars.instance.audioManager.PlaySFX(enemyParams.missSound, 0.8f, transform.position, "WeaponSound", 0, 0.5f);
                        }
                    }
                }
            }
        }
    }
    #endregion

    #region Helper Methods
    /// <summary>
    /// Goes to a given point and can clear a coroutine if given on arrival
    /// </summary>
    /// <param name="newPosition"></param>
    /// <param name="methodToStart"></param>
    /// <param name="doInvoke"></param>
    /// <param name="myCoroutine"></param>
    /// <returns></returns>
    public IEnumerator GoToPosition(Vector3 newPosition, string methodToStart = "", bool doInvoke = false, Coroutine myCoroutine = null)
    {
        bool hasArrived = false;
        if (newPosition != transform.position)
        {
            transform.rotation = Quaternion.LookRotation(newPosition - transform.position, Vector3.up);
            transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
        }

        if (CalculateNewPath(newPosition))
        {
            navMeshAgent.SetDestination(newPosition);
            while (hasArrived == false)
            {
                if (Vector3.Distance(transform.position, newPosition) < 3)
                {
                    //Debug.Log("Arrived");
                    UpdateSprite(idleState);
                    navMeshAgent.SetDestination(transform.position);
                    lastKnownPlayerLocation = transform.position;
                    navMeshAgent.velocity = Vector3.zero;

                    if (methodToStart != "")
                    {
                        if (doInvoke)
                        {
                            Invoke(methodToStart, 0);
                        }
                        else
                        {
                            StartCoroutine(methodToStart);
                        }
                    }

                    if (myCoroutine != null)
                    {
                        StopCoroutine(myCoroutine);
                    }

                    movingCoroutine = null;
                    hasArrived = true;
                }
                if (!hasArrived)
                {
                    UpdateSprite(walkState);
                }
                yield return null;
            }
        }
        else
        {
            //Debug.Log("Arrived");
            UpdateSprite(idleState);
            navMeshAgent.SetDestination(transform.position);
            lastKnownPlayerLocation = transform.position;
            navMeshAgent.velocity = Vector3.zero;

            if (methodToStart != "")
            {
                if (doInvoke)
                {
                    Invoke(methodToStart, 0);
                }
                else
                {
                    StartCoroutine(methodToStart);
                }
            }

            if (myCoroutine != null)
            {
                StopCoroutine(myCoroutine);
            }

            movingCoroutine = null;
            hasArrived = true;
        }
    }

    /// <summary>
    /// Retursn a position within a circle radius from a position
    /// </summary>
    /// <returns></returns>
    public Vector3 GetRandomCirclePosition()
    {
        Vector3 navMeshPos;
        NavMeshHit navMeshHit;

        do
        {
            navMeshPos = RandomPointOnCircleEdge(Random.Range(0, enemyParams.wanderRadius)) + wanderCenteralPoint;

        } while (NavMesh.SamplePosition(navMeshPos, out navMeshHit, 5.0f, NavMesh.AllAreas) != true);

        return navMeshPos;
    }

    /// <summary>
    /// Gets point within circle
    /// </summary>
    /// <param name="radius"></param>
    /// <returns></returns>
    private Vector3 RandomPointOnCircleEdge(float radius)
    {
        var vector2 = Random.insideUnitCircle.normalized * radius;
        return new Vector3(vector2.x, 0, vector2.y);
    }

    /// <summary>
    /// Manually updates AI sprite to given sprite without switching state
    /// </summary>
    /// <param name="newSpriteState"></param>
    public void UpdateSprite(GameObject newSpriteState)
    {
        if (currentState != newSpriteState)
        {
            GameObject currentActiveSprite = currentState.GetComponent<StateSpriteRotation>().currentActiveSprite;
            currentState.SetActive(false);
            currentState = newSpriteState;
            currentState.SetActive(true);
        }
    }

    /// <summary>
    /// Attempts to find a valid path to position
    /// </summary>
    /// <returns></returns>
    bool CalculateNewPath(Vector3 targetPosition)
    {
        NavMeshPath navMeshPath = new NavMeshPath();
        navMeshAgent.CalculatePath(targetPosition, navMeshPath);

        if (navMeshPath.status != NavMeshPathStatus.PathComplete)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
    #endregion
}
