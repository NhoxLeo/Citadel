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
    public MoveSoundsManager moveSoundsManager;

    [Header("AI Audio Settings")]
    public AudioSource movementAudioSource;
    public float minFallTime = 1f;
    public float groundedRayLength = 0.2f;
    public float walkingSpeed = 0.3f;
    public float runningSpeed = 1f;

    [Header("AI States")]
    [Space(10)]
    public GameObject aliveBody;
    public GameObject deadBody;
    public GameObject idleState;
    public GameObject walkState;
    public GameObject attackState;
    public GameObject damageState;
    public GameObject dieState;
    public GameObject gibState;
    public GameObject deadState;
    public GameObject attackParticle;

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
    private float m_inAirTimer = 0f;
    private float idleLockTimer;
    private float walkingTimer;
    private bool canPlayWalkingSound;
    private Coroutine walkingSoundCoroutine;
    private GameObject groundedObject;
    private bool isLerpingFadeOut, isLerpingFadeIn;
    private bool m_isGrounded;
    private bool initialAttackDelay = false;
    private bool wasGibbed;
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
            if (enemyParams.enemyType != Enemy.EnemyType.Friendly)
            {
                CheckForPlayer();
            }
            if (!playerSpotted)
            {
                if (currentTarget == player)
                {
                    currentTarget = null;
                }

                if (!currentlyInState && playerSpottedTimer == 0)
                {
                    initialAttackDelay = false;
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

                if (aiState == AIState.Idle && (enemyParams.enemyBehavior != Enemy.EnemyBehavior.Stationary))
                {
                    idleLockTimer += Time.deltaTime;

                    if(idleLockTimer >= enemyParams.maxIdleStateTime)
                    {
                        currentlyInState = false;
                        playerSpottedTimer = 0;
                        idleLockTimer = 0;
                        SwitchState(AIState.Wander);
                    }
                }
            }
            else
            {
                if (enemyParams.enemyType != Enemy.EnemyType.Friendly)
                {
                    if (!currentTarget)
                    {
                        currentTarget = player;
                        if (enemyParams.equipSound)
                        {
                            GameVars.instance.audioManager.PlaySFX(enemyParams.equipSound, 0.8f, transform.position, "Untagged", 0, 0.5f);
                        }
                        SwitchState(AIState.Attack);
                    }
                }
            }
            if(navMeshAgent.velocity.magnitude < 0.5f && attackingCoroutine == null && damageCoroutine == null && movingCoroutine == null)
            {
                UpdateSprite(idleState);
            }
        }
        else
        {
            if(canPlayWalkingSound)
            {
                canPlayWalkingSound = false;
            }

            if (!isLerpingFadeOut && movementAudioSource.isPlaying)
            {
                walkingSoundCoroutine = StartCoroutine(FadeOutAudioTrack(movementAudioSource, 1));
            }
        }

        CheckIfGrounded();

        if (!m_isGrounded)
        {
            m_inAirTimer += Time.deltaTime;
        }
        else
        {
            if (m_inAirTimer >= 0.3f)
            {
                if (groundedObject)
                {
                    GameVars.instance.audioManager.PlaySFX(moveSoundsManager.GetImpactSound(moveSoundsManager.FindSurfaceAudio(groundedObject.tag)), 0.2f, transform.position, "Untagged", 0, 1f);
                }
            }
            m_inAirTimer = 0f;
        }

        if (navMeshAgent.enabled)
        {
            if (navMeshAgent.velocity.magnitude >= walkingSpeed && m_isGrounded && !isDead)
            {
                walkingTimer += Time.deltaTime;
                canPlayWalkingSound = true;
            }
            else
            {
                walkingTimer = 0;
                canPlayWalkingSound = false;
            }

            if (canPlayWalkingSound && !GameVars.instance.isPaused && !InteractionController.instance.hasPlayerDied)
            {
                movementAudioSource.clip = moveSoundsManager.GetFootSepsSound(moveSoundsManager.FindSurfaceAudio(groundedObject.tag));

                if (!movementAudioSource.isPlaying)
                {
                    if (isLerpingFadeOut)
                    {
                        StopCoroutine(walkingSoundCoroutine);
                        walkingSoundCoroutine = null;
                        isLerpingFadeOut = false;
                    }

                    movementAudioSource.volume = 0.5f;
                    movementAudioSource.Play();
                }
                else
                {
                    if (isLerpingFadeOut)
                    {
                        if (canPlayWalkingSound)
                        {
                            StopCoroutine(walkingSoundCoroutine);
                            isLerpingFadeOut = false;
                            movementAudioSource.volume = 0.5f;
                            movementAudioSource.Play();
                        }
                    }
                    else
                    {
                        movementAudioSource.volume = 0.5f;
                    }
                }

                if (navMeshAgent.velocity.magnitude >= runningSpeed)
                {
                    movementAudioSource.pitch = 1.5f;
                }
                else
                {
                    movementAudioSource.pitch = 0.8f;
                }
            }
            else
            {
                if (movementAudioSource.isPlaying)
                {
                    if (!isLerpingFadeOut)
                    {
                        walkingSoundCoroutine = StartCoroutine(FadeOutAudioTrack(movementAudioSource, 1));
                    }
                    //movementAudioSource.Stop();
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
        Gizmos.DrawSphere(navMeshHit.position, 0.1f);

        //Gizmos.color = Color.blue;
        //Gizmos.DrawLine(transform.position, transform.position + (Vector3.down * ((navMeshAgent.height / 2) + groundedRayLength)));
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
            if(wasGibbed)
            {
                deadState.transform.GetChild(1).gameObject.SetActive(true);
                deadState.transform.GetChild(0).gameObject.SetActive(false);
            }
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

        if (enemyParams.enemyType != Enemy.EnemyType.Friendly)
        {
            movingCoroutine = StartCoroutine(LookAround());
        }

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
                if (enemyParams.enemyType == Enemy.EnemyType.Ranged)
                {
                    if (!initialAttackDelay)
                    {
                        initialAttackDelay = true;
                        yield return new WaitForSeconds(Random.Range(0.1f, 0.5f));
                    }
                }
                else
                {
                    yield return new WaitForSeconds(Random.Range(0.1f, 0.5f));
                }

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
                if (Vector3.Distance(transform.position, player.transform.position) < enemyParams.attackPlayerRadius)
                {
                    if (!initialAttackDelay)
                    {
                        initialAttackDelay = true;
                        yield return new WaitForSeconds(Random.Range(0.1f, 0.5f));
                    }
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
                            //Debug.Log("Can Go To Player");
                            navMeshAgent.SetDestination(player.transform.position);
                            if (Vector3.Distance(transform.position, player.transform.position) < enemyParams.attackPlayerRadius)
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
                            //Debug.Log("Can't Go To Player");
                            if (transform.position != lastKnownPlayerLocation)
                            {
                                if (!CalculateNewPath(lastKnownPlayerLocation))
                                {
                                    //Debug.Log("Can't Go To Player | Can't Go To Last Pos");
                                    navMeshAgent.SetDestination(transform.position);
                                    lastKnownPlayerLocation = transform.position;
                                    navMeshAgent.velocity = Vector3.zero;
                                    hasArrived = true;
                                    UpdateSprite(idleState);
                                }
                                else
                                {
                                    if (CalculateNewPath(lastKnownPlayerLocation) == true)
                                    {
                                        //Debug.Log("Can't Go To Player | Can Go To Last Pos");
                                        navMeshAgent.SetDestination(lastKnownPlayerLocation);
                                        if (Vector3.Distance(transform.position, lastKnownPlayerLocation) < enemyParams.attackPlayerRadius)
                                        {
                                            //Debug.Log("Arrived Local");
                                            navMeshAgent.SetDestination(transform.position);
                                            lastKnownPlayerLocation = transform.position;
                                            navMeshAgent.velocity = Vector3.zero;
                                            hasArrived = true;
                                            UpdateSprite(idleState);
                                        }
                                        if (!hasArrived)
                                        {
                                            UpdateSprite(walkState);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                //Debug.Log("Already There Or No Player");
                                navMeshAgent.SetDestination(transform.position);
                                lastKnownPlayerLocation = transform.position;
                                navMeshAgent.velocity = Vector3.zero;
                                UpdateSprite(idleState);
                                hasArrived = true;
                            }
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
                if (Vector3.Distance(transform.position, player.transform.position) > 6 && lastKnownPlayerLocation != transform.position)
                {
                    //Walk Closer Then Shoot
                    if (enemyParams.agressiveness > 0)
                    {
                        attackingCoroutine = StartCoroutine(GoToPosition(lastKnownPlayerLocation, "", false, attackingCoroutine));
                        yield return new WaitForSeconds(1 * enemyParams.agressiveness);
                    }
                    else
                    {
                        if(Random.Range(0, enemyParams.approachChance) == 0)
                        {
                            attackingCoroutine = StartCoroutine(GoToPosition(lastKnownPlayerLocation, "", false, attackingCoroutine));
                            yield return new WaitForSeconds(1 * 1);
                        }
                    }

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
                        if (enemyParams.agressiveness > 0)
                        {
                            yield return new WaitForSeconds(10 * enemyParams.agressiveness);
                            StopCoroutine(movingCoroutine);
                            navMeshAgent.SetDestination(transform.position);
                            navMeshAgent.velocity = Vector3.zero;
                            movingCoroutine = null;
                        }
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
        if (enemyParams.enemyType != Enemy.EnemyType.Friendly)
        {
            transform.rotation = Quaternion.LookRotation(player.transform.position - transform.position, Vector3.up);
            transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
            SwitchState(AIState.Attack);
        }
        else
        {
            playerSpottedTimer = 0;
            if (enemyParams.enemyBehavior == Enemy.EnemyBehavior.Stationary)
            {
                SwitchState(AIState.Idle);
            }
            else if (enemyParams.enemyBehavior == Enemy.EnemyBehavior.Wander)
            {
                SwitchState(AIState.Wander);
            }
        }
    }

    /// <summary>
    /// Handles AI Death
    /// </summary>
    /// <returns></returns>
    public IEnumerator DieState()
    {
        currentlyInState = true;
        if (wasGibbed)
        {
            UpdateSprite(gibState);
        }
        else
        {
            UpdateSprite(dieState);
        }
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
                        if(CalculateNewPath(navMeshHit.position))
                        {
                            lastKnownPlayerLocation = navMeshHit.position;
                        }
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
                                if (CalculateNewPath(navMeshHit.position))
                                {
                                    lastKnownPlayerLocation = navMeshHit.position;
                                }
                            }
                            transform.rotation = Quaternion.LookRotation(player.transform.position - transform.position, Vector3.up);
                            transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
                            playerSpotted = true;
                        }

                        if(!playerSpotted)
                        {
                            if(collisionCheck.attentionPoint != Vector3.zero)
                            {
                                if (Vector3.Distance(collisionCheck.attentionPoint, transform.position) > 1)
                                {
                                    transform.rotation = Quaternion.LookRotation(collisionCheck.attentionPoint - transform.position, Vector3.up);
                                    transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
                                }
                                collisionCheck.attentionPoint = Vector3.zero;
                            }
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

    public IEnumerator FadeInAudioTrack(AudioSource audioSource, float totalVolume, float lerpDuration)
    {
        float t = 0;

        while (audioSource.volume != totalVolume)
        {
            t += Time.deltaTime / lerpDuration;
            audioSource.volume = Mathf.Lerp(audioSource.volume, totalVolume, t);
            yield return null;
        }
        isLerpingFadeIn = false;
    }

    public IEnumerator FadeOutAudioTrack(AudioSource audioSource, float lerpDuration)
    {
        isLerpingFadeOut = true;
        float defaultVolume = audioSource.volume;
        float t = 0;
        float currentVolume = audioSource.volume;

        while (currentVolume != 0)
        {
            if (audioSource != null)
            {
                t += Time.deltaTime / lerpDuration;
                audioSource.volume = Mathf.Lerp(audioSource.volume, 0, t);
                audioSource.pitch = Mathf.Lerp(audioSource.pitch, 0.8f, t);
                currentVolume = audioSource.volume;
            }
            else
            {
                currentVolume = 0;
            }
            yield return null;
        }

        if (audioSource)
        {
            audioSource.Stop();
        }
        isLerpingFadeOut = false;
    }

    public void CheckIfGrounded()
    {
        Vector3 _origin = transform.position;
        RaycastHit m_hitInfo;
        bool _hitGround = Physics.SphereCast(_origin, 0.5f, Vector3.down, out m_hitInfo, ((navMeshAgent.height / 2) + groundedRayLength), InteractionController.instance.fpsController.groundLayer);
        Debug.DrawRay(_origin, Vector3.down * ((navMeshAgent.height / 2) + groundedRayLength), Color.red);

        m_isGrounded = _hitGround ? true : false;

        if (m_isGrounded)
        {
            groundedObject = m_hitInfo.transform.gameObject;
        }
    }

    public void UpdateNearbyAI()
    {
        RaycastHit[] hitInfo;

        hitInfo = Physics.SphereCastAll(transform.position, enemyParams.viewRange, (player.transform.position - transform.position), enemyParams.attackRange);

        for (int i = 0; i < hitInfo.Length; i++)
        {
            if (hitInfo[i].transform.gameObject.layer == 12)
            {
                if (hitInfo[i].transform.parent)
                {
                    AIController controller = hitInfo[i].transform.parent.gameObject.GetComponent<AIController>();
                    if (controller)
                    {
                        controller.playerSpottedTimer = controller.enemyParams.playerRememberTime;
                        controller.transform.rotation = Quaternion.LookRotation(player.transform.position - controller.transform.position, Vector3.up);
                        controller.transform.rotation = Quaternion.Euler(0, controller.transform.rotation.eulerAngles.y, 0);
                    }
                }
            }
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
        int RandomTurningOffset = Random.Range(0, 360);
        transform.Rotate(0, (RandomTurningOffset), 0);
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
            bool randomDamageState = false;

            if (damageCoroutine == null)
            {
                launchVector = new Vector3(launchVector.x, 0, launchVector.z);
                if ((movingCoroutine == null && attackingCoroutine == null) || InteractionController.instance.newWeapon.weaponParams.weaponType == Weapon.WeaponType.Melee)
                {
                    navMeshAgent.SetDestination(transform.position);
                    navMeshAgent.ResetPath();

                    navMeshAgent.velocity = navMeshAgent.velocity + (launchVector / 2500);
                }

                if (Random.Range(0, 12) == 0 || playerSpottedTimer == 0)
                {
                    navMeshAgent.SetDestination(transform.position);
                    navMeshAgent.ResetPath();

                    StopAllCoroutines();
                    attackingCoroutine = null;
                    movingCoroutine = null;
                    wanderCoroutine = null;
                    attackTimerCoroutine = null;
                    randomDamageState = true;
                }

                playerSpottedTimer = enemyParams.playerRememberTime;
                transform.rotation = Quaternion.LookRotation(player.transform.position - transform.position, Vector3.up);
                transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);

                UpdateNearbyAI();
            }

            if (currentAIHealth > damageToTake)
            {

                if (enemyParams.damageSound && damageSoundObject == null)
                {
                    damageSoundObject = GameVars.instance.audioManager.PlaySFX(enemyParams.damageSound, 0.8f, transform.position, "DamageSound", 0, 0.5f);
                }

                if (randomDamageState)
                {
                    SwitchState(AIState.Damage);
                }
                else
                {
                    if(damageCoroutine != null)
                    {
                        SwitchState(AIState.Attack);
                    }
                }
            }
            else if (currentAIHealth <= damageToTake)
            {
                this.launchVector = launchVector;
                isDead = true;
                if(currentAIHealth >= (enemyParams.health * 0.8) || (InteractionController.instance.newWeapon.weaponParams.weaponName == "Shotgun" && Vector3.Distance(transform.position, player.transform.position) < 7) || InteractionController.instance.newWeapon.weaponParams.weaponType == Weapon.WeaponType.Projectile)
                {
                    wasGibbed = true;
                }
                SwitchState(AIState.Dying);
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
                    rayCast = Physics.SphereCast(transform.position, 0.5f, (player.transform.position - transform.position), out hitInfo, enemyParams.attackRange, enemyParams.damageMask);
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

                    if ((hitInfo.transform.gameObject.layer == 0 || hitInfo.transform.gameObject.layer == 14) && enemyParams.enemyType == Enemy.EnemyType.Ranged)
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
            else
            {
                FireProjectile();
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
                if (Vector3.Distance(transform.position, newPosition) < enemyParams.attackPlayerRadius)
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
    public void FireProjectile()
    {
        Vector3 deviation3D = Random.insideUnitCircle * enemyParams.spreadMaxDivation;
        Quaternion rot;
        if (enemyParams.spreadBasedOnDistance)
        {
            rot = Quaternion.LookRotation(Vector3.forward * (((enemyParams.attackRange) - Vector3.Distance(transform.position, player.transform.position)) - 30) + deviation3D);
        }
        else
        {
            rot = Quaternion.LookRotation(Vector3.forward * (enemyParams.attackRange) + deviation3D);
        }
        Vector3 spawnPos = transform.position + (transform.forward * enemyParams.instantiationDistance) + new Vector3(0, enemyParams.instantiationHeight, 0);
        forwardVector = rot * (player.transform.position - spawnPos);

        GameObject projectile = Instantiate(enemyParams.projectilePrefab, spawnPos, transform.rotation);
        projectile.GetComponent<Projectile>().damage = enemyParams.attackDamage;
        projectile.GetComponent<Projectile>().hitForce = enemyParams.attackForce;
        projectile.GetComponent<Rigidbody>().AddForce(forwardVector * enemyParams.attackForce);
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
