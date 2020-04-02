using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AIGroupManager : MonoBehaviour
{
    #region Inspector Options & Variables
    [Header("AI Manager Type")]
    public ManagerType managerType = ManagerType.OnTrigger;

    [Header ("Settings")]
    [Space(10)]
    [Tooltip("Check if all entities should spawn at once")]
    public bool spawnAllEntitiesAtOnce = true;
    [Tooltip("Check after an AI dies, the spawn point it used should free up for another")]
    public bool allowSpawnPointReuseOnDeath = true;
    [Tooltip("OnTrigger: Delay after trigger active to spawn AI | ReSpawner: Delay after previous AI death to spawn another | OnDelay: Delay after previous AI spawn to spawn another")]
    public float spawnDelay;

    [Header("Event Calls")]
    [Space(10)]
    [Tooltip("Event Invokes On First On Trigger")]
    public UnityEvent OnInitialTrigger;
    [Tooltip("Event Invokes On The Spawn Of A Single AI")]
    public UnityEvent OnSingleAISpawn;
    [Tooltip("Event Invokes On The Death Of A Single AI")]
    public UnityEvent OnSingleAIDeath;
    [Tooltip("Event Invokes On The Death Of All AI")]
    public UnityEvent OnAllAIDeath;

    [Header("Lists")]
    [Space(10)]
    public List<Transform> aiSpawnPositions;
    [Space(5)]
    public List<AIEntity> aiEntities;

    #region Stored Data
    public enum ManagerType { OnTrigger, ReSpawner, OnDelay }
    [HideInInspector]
    public bool hasAllAIDied;

    private AIEntity currentlyAliveEnemy;

    private bool invokedOnAllAIDeath, invokedOnInitialTrigger;
    #endregion
    #endregion

    #region Unity Events
    /// <summary>
    /// Start is called before the first frame update
    /// </summary>
    private void Start()
    {
        //Disable all AI that is waiting for a trigger to spawn
        foreach (AIEntity ai in aiEntities)
        {
            if (ai.waitForSpawnTrigger)
            {
                ai.aiController.gameObject.SetActive(false);
            }
        }

        //Starting Spawning With Delay
        if (managerType == ManagerType.OnDelay)
        {
            OnTrigger();
        }
    }

    /// <summary>
    /// Update is called once per frame
    /// </summary>
    private void Update()
    {
        //Check if AI was just killed
        if(currentlyAliveEnemy != null)
        {
            if(currentlyAliveEnemy.aiController.isDead)
            {
                if (allowSpawnPointReuseOnDeath)
                {
                    currentlyAliveEnemy.spawnPointUsed = null; //Free up any used spawn point
                }
                currentlyAliveEnemy = null;

                if (OnSingleAIDeath != null)
                {
                    OnSingleAIDeath.Invoke();
                }

                if (managerType == ManagerType.ReSpawner)
                {
                    OnTrigger();
                }
            }
        }

        //Check for any surviving AI or AI yet to be spawned
        if (!hasAllAIDied)
        {
            bool foundAliveAi = false;
            for (int i = 0; i < aiEntities.Count; i++)
            {
                if (aiEntities[i].aiController.isDead == false)
                {
                    foundAliveAi = true;
                }
                else
                {
                    if (allowSpawnPointReuseOnDeath)
                    {
                        //Free up any used spawn point because this AI is dead
                        if (aiEntities[i].spawnPointUsed != null)
                        {
                            aiEntities[i].spawnPointUsed = null;
                        }
                    }
                }
            }

            if (!foundAliveAi)
            {
                hasAllAIDied = true;
                if (!invokedOnAllAIDeath)
                {
                    invokedOnAllAIDeath = true;
                    if (OnAllAIDeath != null)
                    {
                        OnAllAIDeath.Invoke();
                    }
                }
            }
        }
    }
    #endregion

    #region Triggers
    /// <summary>
    /// Used to trigger the spawn action for this manager
    /// </summary>
    public void OnTrigger()
    {
        if (!hasAllAIDied && aiEntities != null && aiEntities.Count > 0)
        {
            //Spawn AI
            StartCoroutine(SpawnAI());

            //If this is the first OnTrigger, invoke Event
            if (!invokedOnInitialTrigger)
            {
                invokedOnInitialTrigger = true;
                if (OnInitialTrigger != null)
                {
                    OnInitialTrigger.Invoke();
                }
            }
        }
    }
    #endregion

    #region Main Methods
    /// <summary>
    /// Enumerator used for spawning AI depending on manager type
    /// </summary>
    /// <returns></returns>
    private IEnumerator SpawnAI()
    {
        //Spawn AI
        if (spawnAllEntitiesAtOnce)
        {
            for(int i = 0; i < aiEntities.Count; i++)
            {
                //Wait for Delay
                yield return new WaitForSeconds(spawnDelay);
                Transform spawnPoint = FindValidSpawnPoint();
                if (spawnPoint != null || aiEntities[i].spawnWithoutSpawnPoint == true)
                {
                    SingleSpawn(aiEntities[i], spawnPoint);
                }
            }
        }
        else
        {
            //Wait for Delay
            yield return new WaitForSeconds(spawnDelay);
            Transform spawnPoint = FindValidSpawnPoint();
            AIEntity aiToSpawn = FindValidAIEntity();
            if (spawnPoint != null || aiToSpawn.spawnWithoutSpawnPoint == true)
            {
                SingleSpawn(FindValidAIEntity(), FindValidSpawnPoint());
            }
        }

        //Get started on next spawn if on delay
        if (managerType == ManagerType.OnDelay)
        {
            OnTrigger();
        }
    }
    #endregion

    #region Helper Methods
    /// <summary>
    /// Find a valid AI that both has yet to be spawned and is not dead
    /// </summary>
    /// <returns></returns>
    private AIEntity FindValidAIEntity()
    {
        for (int i = 0; i < aiEntities.Count; i++)
        {
            if(aiEntities[i].aiController.gameObject.activeSelf == false && aiEntities[i].aiController.isDead == false)
            {
                return aiEntities[i];
            }
        }

        return null;
    }

    /// <summary>
    /// Find a valid spawn transform for an AI that has yet to be used
    /// </summary>
    /// <returns></returns>
    private Transform FindValidSpawnPoint()
    {
        Transform pointToReturn;
        foreach (Transform spawnPos in aiSpawnPositions)
        {
            bool alreadyUsed = false;
            for (int i = 0; i < aiEntities.Count; i++)
            {
                if(aiEntities[i].spawnPointUsed == spawnPos)
                {
                    alreadyUsed = true;
                }
            }

            if(!alreadyUsed)
            {
                pointToReturn = spawnPos;
                return pointToReturn;
            }
        }

        //Debug.LogError("ERROR | No Valid Spawn Point Found. Not Enough Spawn Points For Amount Of Enemies To Spawn.");
        return null;
    }

    /// <summary>
    /// Kills only the most recent AI
    /// </summary>
    public void KillMostRecentAI()
    {
        if(currentlyAliveEnemy != null)
        {
            currentlyAliveEnemy.aiController.TakeDamage(9999999);
        }
    }

    /// <summary>
    /// Kills all managed AI
    /// </summary>
    public void KillAllManagedAI()
    {
        for(int i = 0; i < aiEntities.Count; i++)
        {
            aiEntities[i].aiController.TakeDamage(9999999);
        }
    }

    /// <summary>
    /// Kills all AI that are active within the scene
    /// </summary>
    public void KillOnlyActiveAI()
    {
        for (int i = 0; i < aiEntities.Count; i++)
        {
            if (aiEntities[i].aiController.gameObject.activeSelf)
            {
                aiEntities[i].aiController.TakeDamage(9999999);
            }
        }
    }

    /// <summary>
    /// Destroys all ai GameObjects
    /// </summary>
    public void DestroyAllAI()
    {
        for (int i = aiEntities.Count-1; i > -1; i--)
        {
            Destroy(aiEntities[i].aiController.gameObject);
        }
    }

    /// <summary>
    /// Spawn a single AI entity
    /// </summary>
    /// <param name="aiToSpawn"></param>
    /// <param name="spawnPosition"></param>
    private void SingleSpawn(AIEntity aiToSpawn, Transform spawnPosition)
    {
        if (aiToSpawn != null)
        {
            if (aiToSpawn.spawnWithoutSpawnPoint == false)
            {
                aiToSpawn.aiController.gameObject.transform.position = spawnPosition.position;
                aiToSpawn.spawnPointUsed = spawnPosition; //Use a spawn point
            }
            currentlyAliveEnemy = aiToSpawn; //Most recently spawned ai
            aiToSpawn.aiController.gameObject.SetActive(true);
            if (aiToSpawn.spawnParticle)
            {
                Instantiate(aiToSpawn.spawnParticle, aiToSpawn.aiController.gameObject.transform.position, Quaternion.identity);
            }
            if (aiToSpawn.spawnSound.audioClip)
            {
                GameVars.instance.audioManager.PlaySFX(aiToSpawn.spawnSound.audioClip, aiToSpawn.spawnSound.volume, aiToSpawn.aiController.gameObject.transform.position, "Untagged", 0f, 1f);
            }

            if (OnSingleAISpawn != null)
            {
                OnSingleAISpawn.Invoke();
            }
        }
    }
    #endregion
}

[System.Serializable]
public class AIEntity
{
    [Header("AI Settings")]
    #region AI Settings
    [Tooltip("AIController of AI to Manage")]
    public AIController aiController;
    [Tooltip("Set if this enemy should spawn where they currently are within the world")]
    public bool spawnWithoutSpawnPoint = false;
    [Tooltip("Set if this enemy should spawn in instead of already exist within the environment")]
    public bool waitForSpawnTrigger = true;
    [Tooltip("Particle to emit on enemy spawn")]
    public GameObject spawnParticle;
    [Tooltip("Audio to play on enemy spawn")]
    public ReactionAudio spawnSound;
    #endregion

    #region Stored Data
    [HideInInspector]
    public Transform spawnPointUsed;
    #endregion
}
