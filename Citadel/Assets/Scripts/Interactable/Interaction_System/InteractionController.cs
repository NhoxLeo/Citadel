using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VHS
{
    public class InteractionController : MonoBehaviour
    {
        #region Variables    
        [Space, Header("Data")]
        [SerializeField] private InteractionInputData interactionInputData = null;
        [HideInInspector]
        [SerializeField] public InteractionData interactionData = null;

        [Space, Header("Ray Settings")]
        [SerializeField] private float rayDistance = 0f;
        [SerializeField] private float raySphereRadius = 0f;
        [SerializeField] private LayerMask interactableLayer = ~0;

        [Space, Header("References")]
        public Transform grabPoint;
        public Vector3 rayPoint;
        public Animator crossHair;
        public GameObject bulletHolePrefab;
        public GameObject weaponStorageParent;
        public LayerMask bulletLayers;

        [Space, Header("Storage")]
        public GameObject currentInteractingObject;
        public GameObject currentWeapon;
        public List<GameObject> weaponStorge;

        [HideInInspector]
        public List<GameObject> bulletHoles;
        [HideInInspector]
        public bool switchingWeapons = false, wasHolding, switchWeaponLock;
        [HideInInspector]
        public static InteractionController instance; //Singleton
        //[HideInInspector]
        public int currentWeaponIndex = 0;

        #region Private
        private Camera m_cam;

        private bool m_interacting;
        private float m_holdTimer = 0f;

        #endregion

        #endregion

        #region Built In Methods      
        void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            m_cam = FindObjectOfType<Camera>();
            
            if(weaponStorge == null)
            {
                weaponStorge = new List<GameObject>();
            }

            weaponStorageParent.GetComponent<Animator>().SetBool("Equip", true);
            CheckForWeapon();
            //grabPoint.position = new Vector3(0, 0, 1.5f);
        }

        void Update()
        {
            CheckForInteractable();
            CheckForInteractableInput();
            CheckForWeapon();

            if (!wasHolding)
            {
                #region Weapon Shortcuts
                if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    if (weaponStorge.Count > 0)
                    {
                        ChangeWeapon(0, 0);
                    }
                }
                else if (Input.GetKeyDown(KeyCode.Alpha2))
                {
                    if (weaponStorge.Count > 1)
                    {
                        ChangeWeapon(0, 1);
                    }
                }
                else if (Input.GetKeyDown(KeyCode.Alpha3))
                {
                    if (weaponStorge.Count > 2)
                    {
                        ChangeWeapon(0, 2);
                    }
                }
                else if (Input.GetKeyDown(KeyCode.Alpha4))
                {
                    if (weaponStorge.Count > 3)
                    {
                        ChangeWeapon(0, 3);
                    }
                }
                else if (Input.GetKeyDown(KeyCode.Alpha5))
                {
                    if (weaponStorge.Count > 4)
                    {
                        ChangeWeapon(0, 4);
                    }
                }
                else if (Input.GetKeyDown(KeyCode.Alpha6))
                {
                    if (weaponStorge.Count > 5)
                    {
                        ChangeWeapon(0, 5);
                    }
                }
                else if (Input.GetKeyDown(KeyCode.Alpha7))
                {
                    if (weaponStorge.Count > 6)
                    {
                        ChangeWeapon(0, 6);
                    }
                }
                else if (Input.GetKeyDown(KeyCode.Alpha8))
                {
                    if (weaponStorge.Count > 7)
                    {
                        ChangeWeapon(0, 7);
                    }
                }
                else if (Input.GetKeyDown(KeyCode.Alpha9))
                {
                    if (weaponStorge.Count > 8)
                    {
                        ChangeWeapon(0, 8);
                    }
                }
                else if (Input.GetKeyDown(KeyCode.Alpha0))
                {
                    if (weaponStorge.Count > 9)
                    {
                        ChangeWeapon(0, 9);
                    }
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
                else if (Input.GetKeyDown(KeyCode.Q) && !switchingWeapons)
                {
                    ChangeWeapon(1);
                }

                if (bulletHoles.Count > 50)
                {
                    bulletHoles.RemoveAt(0);
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
                wasHolding = false;
                switchingWeapons = false;
            }
        }
        #endregion


        #region Custom methods         
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
            if(weaponStorge != null && weaponStorge.Count > 0)
            {
                if (!switchingWeapons)
                {
                    if (currentWeapon)
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
                if (shiftBy > 0)
                {
                    if (currentWeaponIndex == weaponStorge.Count - 1)
                    {
                        currentWeaponIndex = 0;
                    }
                    else
                    {
                        currentWeaponIndex += shiftBy;
                    }
                }
                else
                {
                    if (currentWeaponIndex == 0)
                    {
                        currentWeaponIndex = weaponStorge.Count - 1;
                    }
                    else
                    {
                        currentWeaponIndex += shiftBy;
                    }
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

        public void AddWeapon(GameObject newWeapon)
        {
            GameObject foundWeapon = SearchForWeapon(newWeapon);
            if (!foundWeapon)
            {
                GameObject weapon = Instantiate(newWeapon, transform.position, Quaternion.Euler(new Vector3(0, 0, 0)));
                weapon.SetActive(false);
                weapon.transform.parent = weaponStorageParent.transform;
                weapon.transform.localPosition = Vector3.zero;
                weapon.transform.localScale = Vector3.one;
                weapon.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));
                weaponStorge.Add(weapon);
                ChangeWeapon(0,weaponStorge.Count - 1);
                //Debug.Log("Added " + newWeapon.GetComponent<WeaponController>().weaponParams.name + " To The Inventory");
            }
            else
            {
                foundWeapon.GetComponent<WeaponController>().totalRounds += foundWeapon.GetComponent<WeaponController>().weaponParams.totalAmmoOnInitialPickup;
                //Debug.Log("Added " + foundWeapon.GetComponent<WeaponController>().weaponParams.totalAmmoOnPickup + " Rounds To " + foundWeapon.GetComponent<WeaponController>().weaponParams.name);
            }
        }

        public void ClearPlayerWeapons()
        {
            if (weaponStorge.Count > 1)
            {
                ChangeWeapon(0, 0);
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
            if(weaponStorge != null && weaponStorge.Count > 0)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(Camera.main.transform.position, Camera.main.transform.position + (Camera.main.transform.forward * weaponStorge[currentWeaponIndex].GetComponent<WeaponController>().weaponParams.attackRange));
                if(weaponStorge[currentWeaponIndex].GetComponent<WeaponController>().weaponParams.weaponType == Weapon.WeaponType.Melee)
                {
                    Gizmos.DrawSphere(Camera.main.transform.position + (Camera.main.transform.forward * weaponStorge[currentWeaponIndex].GetComponent<WeaponController>().weaponParams.attackRange), weaponStorge[currentWeaponIndex].GetComponent<WeaponController>().weaponParams.attackRadius);
                }
            }
        }
    }
}
