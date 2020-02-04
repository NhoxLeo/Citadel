using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VHS;
using UnityEngine.AI;

public class AIController : MonoBehaviour
{
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

    [Header("Debug")]
    [Space(10)]
    public AIState aiState = AIState.Idle;
    public List<AICollisionCheck> aiCollisionChecks;

    [HideInInspector]
    public GameObject currentTarget;
    [HideInInspector]
    public bool isDead;
    //[HideInInspector]
    public float playerSpottedTimer;
    [HideInInspector]
    public enum AIState { Idle, Wander, Attack, Damage, Dying, Dead }

    private GameObject currentState;
    private float currentAIHealth;
    private GameObject player;
    private bool currentlyInState;
    private bool playerSpotted;
    private Vector3 wanderCenteralPoint;
    
    private Vector3 launchVector;
    private Vector3 lastKnownPlayerLocation;
    private NavMeshHit navMeshHit;
    private Coroutine movingCoroutine;
    private Coroutine attackingCoroutine;
    private Coroutine damageCoroutine;
    private Coroutine attackTimerCoroutine;
    private Coroutine wanderCoroutine;


    // Start is called before the first frame update
    void Start()
    {
        player = InteractionController.instance.gameObject.transform.parent.GetChild(1).gameObject;
        currentAIHealth = enemyParams.health;
        currentState = idleState;
        //playerSpottedTimer = enemyParams.playerRememberTime;
        navMeshAgent.speed = enemyParams.walkSpeed;

        wanderCenteralPoint = transform.position;
    }

    // Update is called once per frame
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
                        //Idle Behavior
                    }
                    else if (enemyParams.enemyBehavior == Enemy.EnemyBehavior.Wander)
                    {
                        SwitchState(AIState.Wander);
                        //Wander Behavior
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
                }
            }
            else
            {
                if (!currentTarget)
                {
                    currentTarget = player;
                    if (enemyParams.equipSound)
                    {
                        GameVars.instance.audioManager.PlaySFX(enemyParams.equipSound, 0.8f, transform.position, "WeaponSound");
                    }
                    SwitchState(AIState.Attack);
                }
            }
        }

        transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
    }

    public void TakeDamage(float damageToTake, Vector3 launchVector = new Vector3())
    {
        if (currentAIHealth > 0)
        {
            if (damageCoroutine == null)
            {
                navMeshAgent.SetDestination(transform.position);
                navMeshAgent.velocity = Vector3.zero;

                StopAllCoroutines();
                attackingCoroutine = null;
                movingCoroutine = null;
                wanderCoroutine = null;
                attackTimerCoroutine = null;

                playerSpottedTimer = enemyParams.playerRememberTime;
                transform.rotation = Quaternion.LookRotation(player.transform.position - transform.position, Vector3.up);

            }

            if (currentAIHealth > damageToTake)
            {
                SwitchState(AIState.Damage);
            }
            else if (currentAIHealth <= damageToTake)
            {
                this.launchVector = launchVector;
                isDead = true;
                SwitchState(AIState.Dying);
            }

            currentAIHealth -= damageToTake;
        }
    }

    public IEnumerator DamageState()
    {
        currentlyInState = true;
        UpdateSprite(damageState);
        if (enemyParams.damageSound)
        {
            GameVars.instance.audioManager.PlaySFX(enemyParams.damageSound, 0.8f, transform.position, "DamageSound");
        }
        yield return new WaitForSeconds(enemyParams.damageStateLength);
        UpdateSprite(idleState);
        damageCoroutine = null;
        currentlyInState = false;
        yield return new WaitForSeconds(enemyParams.attackDelayStateLength / 2);
        SwitchState(AIState.Attack);
    }

    public IEnumerator DieState()
    {
        currentlyInState = true;
        UpdateSprite(dieState);
        Rigidbody rigidbody = gameObject.AddComponent<Rigidbody>();
        rigidbody.mass = 100;
        rigidbody.angularDrag = 2;
        navMeshAgent.enabled = false;
        rigidbody.AddForce(launchVector);
        if (enemyParams.deathSound)
        {
            GameVars.instance.audioManager.PlaySFX(enemyParams.deathSound, 0.8f, transform.position, "DamageSound");
        }
        yield return new WaitForSeconds(enemyParams.dyingStateLength);
        SwitchState(AIState.Dead);
        currentlyInState = false;
    }

    public IEnumerator AttackStateTimer()
    {
        if (playerSpottedTimer > 0)
        {
            yield return new WaitForSeconds(1);
            playerSpottedTimer -= 1;
            attackTimerCoroutine = StartCoroutine(AttackStateTimer());
        }
    }

    public void CheckForPlayer()
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
                        playerSpotted = true;
                    }
                }
            }
        }
    }

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
            aliveBody.SetActive(false);
            deadBody.SetActive(true);
        }

        currentState.SetActive(true);
    }

    public void UpdateSprite(GameObject newSpriteState)
    {
        if (currentState != newSpriteState)
        {
            currentState.SetActive(false);
            currentState = newSpriteState;
            currentState.SetActive(true);
        }
    }

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

    private Vector3 RandomPointOnCircleEdge(float radius)
    {
        var vector2 = Random.insideUnitCircle.normalized * radius;
        return new Vector3(vector2.x, 0, vector2.y);
    }

    public IEnumerator AttackState()
    {
        if (currentTarget == player)
        {
            if (movingCoroutine != null)
            {
                StopCoroutine(movingCoroutine);
                navMeshAgent.SetDestination(transform.position);
                navMeshAgent.velocity = Vector3.zero;
                movingCoroutine = null;
            }

            //Debug.Log("ATTACK");
            currentlyInState = true;
            if (enemyParams.enemyType != Enemy.EnemyType.Melee)
            {
                UpdateSprite(idleState);
                EnemyAttack();
                yield return new WaitForSeconds(enemyParams.attackStateLength);
                UpdateSprite(idleState);
                if (enemyParams.reloadSound)
                {
                    GameVars.instance.audioManager.PlaySFX(enemyParams.reloadSound, 0.8f, transform.position, "WeaponSound");
                }
                yield return new WaitForSeconds(enemyParams.attackDelayStateLength);
            }
            else
            {
                bool hasArrived = false;
                if (Vector3.Distance(transform.position, player.transform.position) < 3)
                {
                    EnemyAttack();
                    yield return new WaitForSeconds(enemyParams.attackStateLength);
                    UpdateSprite(idleState);
                    if (enemyParams.reloadSound)
                    {
                        GameVars.instance.audioManager.PlaySFX(enemyParams.reloadSound, 0.8f, transform.position, "WeaponSound");
                    }
                    yield return new WaitForSeconds(enemyParams.attackDelayStateLength);
                }
                else
                {
                    while (hasArrived == false)
                    {
                        transform.rotation = Quaternion.LookRotation(player.transform.position - transform.position, Vector3.up);
                        navMeshAgent.SetDestination(player.transform.position);
                        if (Vector3.Distance(transform.position, player.transform.position) < 3)
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
                                GameVars.instance.audioManager.PlaySFX(enemyParams.reloadSound, 0.8f, transform.position, "WeaponSound");
                            }
                            yield return new WaitForSeconds(enemyParams.attackDelayStateLength);
                        }
                        if (!hasArrived)
                        {
                            UpdateSprite(walkState);
                        }
                        transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
                        yield return null;
                    }

                    yield return new WaitUntil(() => (hasArrived == true));
                }
            }
        }

        attackingCoroutine = null;

        if (currentTarget == player)
        {
            if (enemyParams.enemyBehavior == Enemy.EnemyBehavior.Stationary)
            {
                //Keep Shooting
                SwitchState(AIState.Attack);
            }
            else if (enemyParams.enemyBehavior == Enemy.EnemyBehavior.Wander)
            {
                if (Vector3.Distance(transform.position, player.transform.position) > 3)
                {
                    //Walk Closer Then Shoot
                    attackingCoroutine = StartCoroutine(GoToPosition(lastKnownPlayerLocation, "" , false, attackingCoroutine));
                    yield return new WaitForSeconds(1 * enemyParams.agressiveness);
                    StopCoroutine(attackingCoroutine);
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
            //lastKnownPlayerLocation
            if (transform.position != lastKnownPlayerLocation)
            {
                if (movingCoroutine == null)
                {
                    //Debug.Log("Hunting Player");
                    movingCoroutine = StartCoroutine(GoToPosition(lastKnownPlayerLocation, "", false, movingCoroutine));
                    if(enemyParams.agressiveness < 1)
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

    public IEnumerator GoToPosition(Vector3 newPosition, string methodToStart = "", bool doInvoke = false, Coroutine myCoroutine = null)
    {
        bool hasArrived = false;
        transform.rotation = Quaternion.LookRotation(newPosition - transform.position, Vector3.up);
        navMeshAgent.SetDestination(newPosition);

        while (hasArrived == false)
        {
            if (Vector3.Distance(transform.position, newPosition) < 2)
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
                        //Debug.Log("Attacking");
                        StartCoroutine(methodToStart);
                    }
                }

                if (myCoroutine != null)
                {
                    StopCoroutine(myCoroutine);
                }

                movingCoroutine = null;
                hasArrived = true;
                UpdateSprite(idleState);
            }
            if (!hasArrived)
            {
                UpdateSprite(walkState);
            }
            transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
            yield return null;
        }
    }

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

    public void EnemyAttack()
    {
        UpdateSprite(attackState);
        GameVars.instance.audioManager.PlaySFX(enemyParams.attackSound, 0.8f, transform.position, "WeaponSound");
        if (attackParticle)
        {
            Instantiate(attackParticle, transform.position, Quaternion.identity);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * 2);

        Gizmos.DrawSphere(navMeshHit.position, 4);
    }
}
