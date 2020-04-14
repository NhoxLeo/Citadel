using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
namespace VHS
{
    public class InteractionController : MonoBehaviour
    {
        #region Variables    
        [Space, Header("Data")]
        [SerializeField] public InteractionInputData interactionInputData = null;
        [HideInInspector]
        [SerializeField] public InteractionData interactionData = null;
        public FirstPersonController fpsController;

        [Space, Header("Ray Settings")]
        [SerializeField] private float rayDistance = 0f;
        [SerializeField] private float raySphereRadius = 0f;
        [SerializeField] private LayerMask interactableLayer = ~0;

        [Space, Header("Health")]
        public Animator playerDamageAnimator;
        public Animator playerDeathAnimator;
        public Image vignette;
        public float playerHealth = 100;
        public float playerRegenIncrement = 5;
        public float playerRegenRatePerSec = 1;
        public float playerDamageSoundDelay = 0.3f;
        public AudioClip playerDamageAudio;

        [Space, Header("References")]
        public Transform grabPoint;
        public Vector3 rayPoint;
        public Animator crossHair;
        public GameObject bulletHolePrefab;
        public GameObject bloodSplatPrefab;
        public GameObject weaponStorageParent;
        public LayerMask bulletLayers;
        public LayerMask punchLayers;
        public GameObject backupAudioManager;
        public HUDController hudController;
        public Animator teleportAnimator;

        [Space, Header("Storage")]
        public GameObject currentInteractingObject;
        public GameObject currentWeapon;
        public List<GameObject> weaponStorge;

        [HideInInspector]
        public List<GameObject> bulletHoles;
        [HideInInspector]
        public List<GameObject> bloodSplats;
        [HideInInspector]
        public bool switchingWeapons = false, wasHolding, switchWeaponLock;
        [HideInInspector]
        public static InteractionController instance; //Singleton
        [HideInInspector]
        public int currentWeaponIndex = 0;
        [HideInInspector]
        public float maxHealth;
        [HideInInspector]
        public bool hasPlayerDied;
        [HideInInspector]
        public WeaponController newWeapon;

        #region Private
        private Camera m_cam;

        private bool m_interacting;
        private float m_holdTimer = 0f;
        private bool canPlayDamageSound = true;

        private Coroutine regenCoroutine, vignetteCoroutine;
        private bool waitingToUpdateUI;

        private float damageSoundTimer;

        #endregion

        #endregion

        #region Built In Methods      
        void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            m_cam = Camera.main;
            
            if(weaponStorge == null)
            {
                weaponStorge = new List<GameObject>();
            }

            weaponStorageParent.GetComponent<Animator>().SetBool("Equip", true);
            CheckForWeapon();
            maxHealth = playerHealth;
        }

        private void Start()
        {
            if (GameVars.instance == null)
            {
                Instantiate(backupAudioManager, Vector3.zero, Quaternion.identity);
            }
        }

        void Update()
        {
            CheckForInteractable();
            CheckForInteractableInput();
            CheckForWeapon();

            if (!wasHolding)
            {
                #region Weapon Shortcuts
                if (switchWeaponLock == false)
                {
                    if (Input.GetKeyDown(KeyCode.Alpha1))
                    {
                        int foundIndex = findWeapon("Axe", false);

                        if (foundIndex != -1)
                        {
                            ChangeWeapon(0, foundIndex);
                        }
                        else
                        {
                            findWeapon("Fist", true);
                        }
                    }
                    else if (Input.GetKeyDown(KeyCode.Alpha2))
                    {
                        findWeapon("Pistol", true);
                    }
                    else if (Input.GetKeyDown(KeyCode.Alpha3))
                    {
                        findWeapon("Shotgun", true);
                    }
                    else if (Input.GetKeyDown(KeyCode.Alpha4))
                    {
                        findWeapon("Machine Gun", true);
                    }
                    else if (Input.GetKeyDown(KeyCode.Alpha5))
                    {
                        findWeapon("Pulse Rifle", true);
                    }
                    else if (Input.GetKeyDown(KeyCode.Alpha6))
                    {
                        findWeapon("Grenade", true);
                    }
                    else if (Input.GetKeyDown(KeyCode.Alpha7))
                    {
                        findWeapon("Rocket Launcher", true);
                    }
                    else if (Input.GetKeyDown(KeyCode.Alpha8))
                    {
                        findWeapon("Disruptor", true);
                    }
                    #endregion

                    if (Input.GetAxis("Mouse ScrollWheel") > 0 && !switchWeaponLock)
                    {
                        ChangeWeapon(1);
                    }
                    else if (Input.GetAxis("Mouse ScrollWheel") < 0 && !switchWeaponLock)
                    {
                        ChangeWeapon(-1);
                    }
                    else if (Input.GetKeyDown((KeyCode)System.Enum.Parse(typeof(KeyCode), GameVars.instance.saveManager.INPUT_QUICKCYCLE)) && !switchingWeapons)
                    {
                        ChangeWeapon(1);
                    }
                }

                if (bulletHoles.Count > 50)
                {
                    bulletHoles.RemoveAt(0);
                }

                if (bloodSplats.Count > 50)
                {
                    bloodSplats.RemoveAt(0);
                }
            }

            if(currentInteractingObject && !switchingWeapons)
            {
                switchingWeapons = true;
                wasHolding = true;
                weaponStorageParent.GetComponent<Animator>().SetBool("Equip", false);
                weaponStorageParent.GetComponent<Animator>().SetBool("Unequip", true);
            }
            else if (!currentInteractingObject && wasHolding)
            {
                weaponStorageParent.GetComponent<Animator>().SetBool("Equip", true);
                weaponStorageParent.GetComponent<Animator>().SetBool("Unequip", false);
                wasHolding = false;
                switchingWeapons = false;
            }

            if (playerHealth < maxHealth)
            {
                if (vignette.color != new Color(0, 0, 0, (((playerHealth / maxHealth) - 1) * -1)))
                {
                    if (vignetteCoroutine == null)
                    {
                        vignetteCoroutine = StartCoroutine(LerpDamage());
                    }
                }
                if(regenCoroutine == null)
                {
                    regenCoroutine = StartCoroutine(RegenHealth());
                }

                if(playerHealth <= 0)
                {
                    playerHealth = 0;
                    StartCoroutine(PlayerDie());
                }
            }

            if(switchWeaponLock)
            {
                waitingToUpdateUI = true;
            }
            else
            {
                if(waitingToUpdateUI)
                {
                    if(currentWeaponIndex < 0)
                    {
                        currentWeaponIndex = weaponStorge.Count - 1;
                    }
                    if (currentWeaponIndex > weaponStorge.Count-1)
                    {
                        currentWeaponIndex = 0;
                    }

                    newWeapon = weaponStorge[currentWeaponIndex].GetComponent<WeaponController>();
                    hudController.DoUiFade(HUDController.FadeState.IN);
                    waitingToUpdateUI = false;
                }
                else
                {
                    if(newWeapon.weaponParams.weaponType == Weapon.WeaponType.Melee)
                    {
                        hudController.UpdateWeaponInfo("N/A", "   ");
                    }
                    else
                    {
                        if (!newWeapon.weaponParams.doesNeedReload)
                        {
                            hudController.UpdateWeaponInfo((newWeapon.totalRounds / newWeapon.weaponParams.totalRoundsPerShot).ToString(),"   ");
                        }
                        else
                        {
                            hudController.UpdateWeaponInfo(newWeapon.totalRounds.ToString(), newWeapon.currentLoadedRounds.ToString());
                        }
                    }
                }
            }

            if(!canPlayDamageSound)
            {
                damageSoundTimer += Time.deltaTime;
                if(damageSoundTimer >= playerDamageSoundDelay)
                {
                    canPlayDamageSound = true;
                    damageSoundTimer = 0;
                }
            }
        }
        #endregion


        #region Custom methods    
        public IEnumerator LerpDamage()
        {
            float t = 0f;
            while (vignette.color != new Color(0, 0, 0, (((playerHealth / maxHealth) - 1) * -1)))
            {
                t += Time.deltaTime / 3;
                vignette.color = Color.Lerp(vignette.color, (new Color(0, 0, 0, (((playerHealth / maxHealth) - 1) * -1))), t);

                if (vignette.color == new Color(0, 0, 0, (((playerHealth / maxHealth) - 1) * -1)))
                {
                    vignette.color = new Color(0, 0, 0, (((playerHealth / maxHealth) - 1) * -1));
                    vignetteCoroutine = null;
                }

                yield return null;
            }
            vignette.color = new Color(0, 0, 0, (((playerHealth / maxHealth) - 1) * -1));
        }

        public int findWeapon(string weaponName, bool doSwap)
        {
            for(int i = 0; i < weaponStorge.Count; i++)
            {
                if(weaponStorge[i].GetComponent<WeaponController>().weaponParams.weaponName == weaponName)
                {
                    if (doSwap)
                    {
                        ChangeWeapon(0, i);
                    }
                    return i;
                }
            }
            return -1;
        }
        public IEnumerator PlayerDie()
        {
            if (!hasPlayerDied)
            {
                hasPlayerDied = true;
                playerHealth = 0;
                hudController.DoUiFade(HUDController.FadeState.OUT, true);
                playerDamageAnimator.SetBool("HasDied", true);
                playerDeathAnimator.SetBool("Die", true);

                switchingWeapons = true;
                weaponStorageParent.GetComponent<Animator>().SetBool("Equip", false);
                weaponStorageParent.GetComponent<Animator>().SetBool("Unequip", true);
                ClearPlayerWeapons(false);

                AudioSource[] audioSources = GameObject.FindObjectsOfType<AudioSource>();
                for (int i = 0; i < audioSources.Length; i++)
                {
                    if (audioSources[i] != null)
                    {
                        if (audioSources[i].gameObject.transform.parent != gameObject.transform.parent)
                        {
                            StartCoroutine(GameVars.instance.audioManager.FadeOutAudioTrack(audioSources[i], 7));
                        }
                    }
                }
                yield return new WaitForSeconds(5);
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }

        void CheckForInteractable()
        {
            Ray _ray = new Ray(m_cam.transform.position, m_cam.transform.forward);
            RaycastHit _hitInfo;

            bool _hitSomething = Physics.SphereCast(_ray, raySphereRadius, out _hitInfo, rayDistance, interactableLayer);

            if (_hitSomething)
            {
                InteractableBase _interactable = _hitInfo.transform.GetComponent<InteractableBase>();
                if (_hitInfo.point != null && _interactable != null)
                {
                    rayPoint = _interactable.transform.InverseTransformPoint(_hitInfo.point);
                }

                if (_interactable != null)
                {
                    if (interactionData.IsEmpty())
                    {
                        interactionData.Interactable = _interactable;

                        if (crossHair)
                        {
                            crossHair.SetBool("Open", true);
                        }
                    }
                    else
                    {
                        if (!interactionData.IsSameInteractable(_interactable))
                        {
                            interactionData.Interactable = _interactable;

                            if (crossHair)
                            {
                                crossHair.SetBool("Open", true);
                            }
                        }
                    }
                }
            }
            else
            {
                interactionData.ResetData(crossHair);
            }

            Debug.DrawRay(_ray.origin, _ray.direction * rayDistance, _hitSomething ? Color.green : Color.red);
        }

        void CheckForWeapon()
        {
            if (!hasPlayerDied)
            {
                if (weaponStorge != null && weaponStorge.Count > 0)
                {
                    if (!switchingWeapons)
                    {
                        if (currentWeapon && weaponStorge[currentWeaponIndex])
                        {
                            if (currentWeapon != weaponStorge[currentWeaponIndex])
                            {
                                currentWeapon.SetActive(false);
                                currentWeapon = weaponStorge[currentWeaponIndex];
                                currentWeapon.SetActive(true);
                            }
                        }
                        else
                        {
                            currentWeapon = weaponStorge[currentWeaponIndex];
                            currentWeapon.SetActive(true);
                        }

                        if (newWeapon == null)
                        {
                            newWeapon = currentWeapon.GetComponent<WeaponController>();
                        }
                    }
                }
            }
        }

        public void ChangeWeapon(int shiftBy, int toIndex = -1)
        {
            if (weaponStorge.Count > 1 && toIndex != currentWeaponIndex)
            {   
                if (switchWeaponLock)
                {
                    StartCoroutine(SwitchWeapons(shiftBy, toIndex, true));
                }
                else
                {
                    switchingWeapons = true;
                    switchWeaponLock = true;
                    StartCoroutine(SwitchWeapons(shiftBy, toIndex, false));
                }             
            }
        }

        public void TakeDamage(float damage)
        {
            if (!hasPlayerDied)
            {
                float vignetteAlpha = (((((playerHealth / maxHealth) - 1) * -1) + 0.1f) * 2);
                if(vignetteAlpha > 0.8f)
                {
                    vignetteAlpha = 0.8f;
                }

                if(vignetteAlpha < 0.3)
                {
                    vignetteAlpha = 0.3f;
                }

                vignette.color = new Color(0, 0, 0, vignetteAlpha);
                if(vignetteCoroutine != null)
                {
                    StopCoroutine(vignetteCoroutine);
                }
                vignetteCoroutine = StartCoroutine(LerpDamage());
                damage = damage + ((int)Mathf.Ceil((GameVars.instance.difficultyManager.enemyDamagePercent * damage) * GameVars.instance.difficultyManager.GetDifficultyScaler() * -1));
                playerHealth -= damage;
                GameVars.instance.totalDamageTaken += damage;
                if (playerDamageAudio && canPlayDamageSound)
                {
                    canPlayDamageSound = false;
                    GameVars.instance.audioManager.PlaySFX(playerDamageAudio, 1, transform.position);
                }

                if (!hasPlayerDied && playerHealth > 0)
                {
                    hudController.DoUiFade(HUDController.FadeState.IN);
                }
            }
        }

        public IEnumerator RegenHealth()
        {
            if(playerHealth < maxHealth && !hasPlayerDied)
            {
                yield return new WaitForSeconds(playerRegenRatePerSec);
                if(!hasPlayerDied)
                {
                    playerHealth += playerRegenIncrement + ((int)Mathf.Ceil((GameVars.instance.difficultyManager.regenSpeedPercent * playerRegenIncrement) * GameVars.instance.difficultyManager.GetDifficultyScaler()));
                }

                if (playerHealth >= maxHealth)
                {
                    playerHealth = maxHealth;
                    regenCoroutine = null;
                }
                else if(playerHealth < maxHealth)
                {
                    StartCoroutine(RegenHealth());
                }
            }
        }

        public IEnumerator SwitchWeapons(int shiftBy, int toIndex = -1, bool toWait = false)
        {
            if(toWait)
            {
                yield return new WaitUntil(() => (switchWeaponLock == false));
                switchingWeapons = true;
                switchWeaponLock = true;
            }

            weaponStorageParent.GetComponent<Animator>().SetBool("Unequip", true);
            weaponStorageParent.GetComponent<Animator>().SetBool("Equip", false);
            yield return new WaitForSeconds(0.2f);

            if (toIndex == -1)
            {
                bool foundWeaponWithAmmo = false;
                bool allowMelee = false;
                int startingIndex = currentWeaponIndex;

                if (shiftBy > 0)
                {
                    startingIndex++;
                    do
                    {
                        if (startingIndex > weaponStorge.Count - 1)
                        {
                            startingIndex = 0;
                        }

                        WeaponController weaponToSwap = weaponStorge[startingIndex].GetComponent<WeaponController>();
                        //Debug.Log(weaponToSwap.weaponParams.name + " | Total Rounds: " + weaponToSwap.totalRounds + " | Loaded Rounds: " + weaponToSwap.currentLoadedRounds);
                        if ((weaponToSwap.totalRounds > 0 || weaponToSwap.currentLoadedRounds > 0 && weaponToSwap.weaponParams.doesNeedReload) || (weaponToSwap.weaponParams.weaponType == Weapon.WeaponType.Melee))
                        {
                            currentWeaponIndex = startingIndex;
                            foundWeaponWithAmmo = true;
                        }
                        else
                        {
                            startingIndex++;
                        }
                    } while (foundWeaponWithAmmo == false);
                }
                else if(shiftBy < 0)
                {
                    startingIndex--;
                    do
                    {
                        if (startingIndex < 0)
                        {
                            startingIndex = weaponStorge.Count - 1;
                        }

                        WeaponController weaponToSwap = weaponStorge[startingIndex].GetComponent<WeaponController>();
                        //Debug.Log(weaponToSwap.weaponParams.name + " | Total Rounds: " + weaponToSwap.totalRounds + " | Loaded Rounds: " + weaponToSwap.currentLoadedRounds);
                        if ((weaponToSwap.totalRounds > 0 || weaponToSwap.currentLoadedRounds > 0 && weaponToSwap.weaponParams.doesNeedReload) || (weaponToSwap.weaponParams.weaponType == Weapon.WeaponType.Melee))
                        {
                            currentWeaponIndex = startingIndex;
                            foundWeaponWithAmmo = true;
                        }
                        else
                        {
                            startingIndex--;
                        }
                    } while (foundWeaponWithAmmo == false);
                }
                else
                {
                    startingIndex++;
                    do
                    {
                        if (startingIndex > weaponStorge.Count - 1)
                        {
                            startingIndex = 0;
                        }

                        if (startingIndex == currentWeaponIndex)
                        {
                            allowMelee = true;
                        }

                        WeaponController weaponToSwap = weaponStorge[startingIndex].GetComponent<WeaponController>();
                        //Debug.Log(weaponToSwap.weaponParams.name + " | Total Rounds: " + weaponToSwap.totalRounds + " | Loaded Rounds: " + weaponToSwap.currentLoadedRounds);
                        if (((weaponToSwap.totalRounds > 0 || weaponToSwap.currentLoadedRounds > 0 && weaponToSwap.weaponParams.doesNeedReload) && weaponToSwap.weaponParams.weaponType != Weapon.WeaponType.Melee) || (allowMelee == true && weaponToSwap.weaponParams.weaponType == Weapon.WeaponType.Melee))
                        {
                            currentWeaponIndex = startingIndex;
                            foundWeaponWithAmmo = true;
                        }
                        else
                        {
                            startingIndex++;
                        }
                    } while (foundWeaponWithAmmo == false);
                }
            }
            else
            {
                currentWeaponIndex = toIndex;
            }
            switchingWeapons = false;
            weaponStorageParent.GetComponent<Animator>().SetBool("Equip", true);
            weaponStorageParent.GetComponent<Animator>().SetBool("Unequip", false);
            yield return new WaitForSeconds(0.45f);
            switchWeaponLock = false;
        }

        public void AddWeapon(GameObject newWeapon, int ammoAmount)
        {
            hudController.DoUiFade(HUDController.FadeState.IN);
            GameObject foundWeapon = SearchForWeapon(newWeapon);
            if (!foundWeapon)
            {
                GameObject weapon = Instantiate(newWeapon, transform.position, Quaternion.Euler(new Vector3(0, 0, 0)));
                weapon.SetActive(false);
                weapon.transform.parent = weaponStorageParent.transform;
                weapon.transform.localPosition = Vector3.zero;
                weapon.transform.localScale = Vector3.one;
                weapon.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));
                if (ammoAmount != 0)
                {
                    weapon.GetComponent<WeaponController>().totalRounds = ammoAmount;
                }
                weaponStorge.Add(weapon);
                ChangeWeapon(0,weaponStorge.Count - 1);
                //Debug.Log("Added " + newWeapon.GetComponent<WeaponController>().weaponParams.name + " To The Inventory");
            }
            else
            {
                WeaponController foundWeaponController = foundWeapon.GetComponent<WeaponController>();
                if (ammoAmount == 0)
                {
                    foundWeaponController.totalRounds += foundWeaponController.weaponParams.totalAmmoOnAmmoPickup + ((int)Mathf.Ceil((GameVars.instance.difficultyManager.ammoPickUpPercent * foundWeaponController.weaponParams.totalAmmoOnAmmoPickup) * GameVars.instance.difficultyManager.GetDifficultyScaler()));
                }
                else
                {
                    foundWeaponController.totalRounds += ammoAmount + ((int)Mathf.Ceil((GameVars.instance.difficultyManager.ammoPickUpPercent * ammoAmount) * GameVars.instance.difficultyManager.GetDifficultyScaler()));
                }
                //Debug.Log("Added " + foundWeapon.GetComponent<WeaponController>().weaponParams.totalAmmoOnPickup + " Rounds To " + foundWeapon.GetComponent<WeaponController>().weaponParams.name);
            }
        }

        public void ClearPlayerWeapons(bool doChange = true)
        {
            if (weaponStorge.Count > 1)
            {
                if (doChange)
                {
                    ChangeWeapon(0, 0);
                }
                for (int i = weaponStorge.Count-1; i > 0; i--)
                {
                    if (weaponStorge[i] != null)
                    {
                        Destroy(weaponStorge[i]);
                        weaponStorge.RemoveAt(i);
                    }
                }
            }
        }

        public GameObject SearchForWeapon(GameObject newWeapon)
        {
            foreach(GameObject weapon in weaponStorge)
            {
                if(weapon.GetComponent<WeaponController>().weaponParams.name == newWeapon.GetComponent<WeaponController>().weaponParams.name)
                {
                    return weapon;
                }
            }

            return null;
        }

        void CheckForInteractableInput()
        {
            if (interactionData.IsEmpty())
                return;

            if (interactionInputData.InteractedClicked)
            {
                m_interacting = true;
                m_holdTimer = 0f;
            }

            if (interactionInputData.InteractedReleased)
            {
                m_interacting = false;
                m_holdTimer = 0f;
            }

            if (m_interacting)
            {
                if (!interactionData.Interactable.IsInteractable)
                    return;

                if (interactionData.Interactable.HoldInteract)
                {
                    m_holdTimer += Time.deltaTime;

                    if (m_holdTimer >= interactionData.Interactable.HoldDuration)
                    {
                        interactionData.Interact(rayPoint, grabPoint);
                        m_interacting = false;
                    }
                }
                else
                {
                    interactionData.Interact(rayPoint, grabPoint);
                    m_interacting = false;
                }
            }
        }
        #endregion

        private void OnDrawGizmos()
        {
            if(weaponStorge != null && weaponStorge.Count > 0 && weaponStorge[currentWeaponIndex])
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(Camera.main.transform.position, Camera.main.transform.position + (Camera.main.transform.forward * weaponStorge[currentWeaponIndex].GetComponent<WeaponController>().weaponParams.attackRange));
                if(weaponStorge[currentWeaponIndex].GetComponent<WeaponController>().weaponParams.weaponType == Weapon.WeaponType.Melee)
                {
                    Gizmos.DrawSphere(Camera.main.transform.position, weaponStorge[currentWeaponIndex].GetComponent<WeaponController>().weaponParams.attackRadius);
                }
            }
        }
    }
}
