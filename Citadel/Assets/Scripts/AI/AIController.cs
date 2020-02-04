using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VHS;
using UnityEngine.AI;

public class AIController : MonoBehaviour
{
    [Header("AI Information")]
    public Enemy enemyParams;
    public FieldOfView fieldOfView;
    public NavMeshAgent navMeshAgent;
    public GameObject aliveBody;
    public GameObject deadBody;
    public GameObject idleState;
    public GameObject walkState;
    public GameObject attackState;
    public GameObject damageState;
    public GameObject dieState;
    public GameObject deadState;
    public GameObject attackParticle;
    public AIState aiState = AIState.Idle;
    public List<AICollisionCheck> aiCollisionChecks;

    [HideInInspector]
    public GameObject currentTarget;
    [HideInInspector]
    public bool isDead;
    [HideInInspector]
    public enum AIState { Idle, Wander, Attack, Damage, Dying, Dead }

    private GameObject currentState;
    private float currentAIHealth;
    private GameObject player;
    private bool currentlyInState;
    private bool playerSpotted;
    private float playerSpottedTimer;
    private Vector3 launchVector;
    private Vector3 lastKnownPlayerLocation;
    private NavMeshHit navMeshHit;
    private Coroutine movingCoroutine;
    private Coroutine attackingCoroutine;
    private Coroutine damageCoroutine;


    // Start is called before the first frame update
    void Start()
    {
        player = InteractionController.instance.gameObject.transform.parent.GetChild(1).gameObject;
        currentAIHealth = enemyParams.health;
        currentState = idleState;
        playerSpottedTimer = enemyParams.playerRememberTime;
        navMeshAgent.speed = enemyParams.walkSpeed;
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

                if (!currentlyInState)
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
                            SwitchState(AIState.Idle);
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
                                    Debug.Log("Look Around");
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
                    SwitchState(AIState.Attack);
                }
            }
        }

        transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
    }

    public void TakeDamage(float damageToTake, Vector3 launchVector = new Vector3())
    {
        if(currentAIHealth > 0)
        {
            if (damageCoroutine == null)
            {
                navMeshAgent.SetDestination(transform.position);
                navMeshAgent.velocity = Vector3.zero;

                StopAllCoroutines();
                attackingCoroutine = null;
                movingCoroutine = null;
            }

            if(currentAIHealth > damageToTake)
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
        if(enemyParams.damageSound)
        {
            GameVars.instance.audioManager.PlaySFX(enemyParams.damageSound, 0.8f, transform.position, "DamageSound");
        }
        yield return new WaitForSeconds(enemyParams.damageStateLength);
        UpdateSprite(idleState);
        damageCoroutine = null;
        currentlyInState = false;
        yield return new WaitForSeconds(enemyParams.attackDelayStateLength/2);
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

    public void CheckForPlayer()
    {
        playerSpotted = false;
        if (fieldOfView)
        {
            if(fieldOfView.visibleTargets.Count > 0)
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

        if(aiCollisionChecks != null)
        {
            if(aiCollisionChecks.Count > 0)
            {
                foreach(AICollisionCheck collisionCheck in aiCollisionChecks)
                {
                    if(collisionCheck.isPlayerColliding)
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
                currentState = walkState;
            }
            else if (newState == AIState.Attack)
            {
                if (attackingCoroutine == null)
                {
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
        if(currentState != newSpriteState)
        {
            currentState.SetActive(false);
            currentState = newSpriteState;
            currentState.SetActive(true);
        }
    }

    public IEnumerator AttackState()
    {
        if (currentTarget == player)
        {
            if(movingCoroutine != null)
            {
                StopCoroutine(movingCoroutine);
                navMeshAgent.SetDestination(transform.position);
                navMeshAgent.velocity = Vector3.zero;
                movingCoroutine = null;
            }

            //Debug.Log("ATTACK");
            currentlyInState = true;
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

        attackingCoroutine = null;

        if(currentTarget == player)
        {
            if (enemyParams.enemyBehavior == Enemy.EnemyBehavior.Stationary)
            {
                //Keep Shooting
                SwitchState(AIState.Attack);
            }
            else if (enemyParams.enemyBehavior == Enemy.EnemyBehavior.Wander)
            {
                //Walk Closer Then Shoot
                attackingCoroutine = StartCoroutine(GoToPosition(lastKnownPlayerLocation));
                yield return new WaitForSeconds(1);
                StopCoroutine(attackingCoroutine);
                navMeshAgent.SetDestination(transform.position);
                navMeshAgent.velocity = Vector3.zero;
                attackingCoroutine = null;
                SwitchState(AIState.Attack);
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
                    movingCoroutine = StartCoroutine(GoToPosition(lastKnownPlayerLocation));
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

    public IEnumerator GoToPosition(Vector3 newPosition, string methodToStart = "", bool doInvoke = false)
    {
        bool hasArrived = false;
        //Debug.Log("Going To Position");
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

                movingCoroutine = null;
                hasArrived = true;
            }
            UpdateSprite(walkState);
            transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
            yield return null;
        }
    }

    public IEnumerator LookAround()
    {
        int lookIncrement = 8;
        float turnDelay = 0.5f;
        for(int i = 0; i < 360; i += (360/lookIncrement))
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
        Gizmos.DrawLine(transform.position, transform.position + transform.forward*2);

        Gizmos.DrawSphere(navMeshHit.position, 4);
    }
}
