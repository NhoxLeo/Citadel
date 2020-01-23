using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VHS
{
    [RequireComponent(typeof(Rigidbody))]
    public class Pickable : InteractableBase, IPickable
    {
        private Rigidbody rigid;
        private Transform playerGrip;
        private Vector3 contactPoint;
        private ConfigurableJoint joint;
        private GameObject stuckOnObject;
        private float t;
        private float timeToReachTarget = 1;

        private void Awake()
        {
            rigid = GetComponent<Rigidbody>();
        }

        public override void OnInteract(Vector3 contactPoint, Transform playerGrip = null)
        {
            if(!Picked)
            {
                this.playerGrip = playerGrip;
                this.contactPoint = contactPoint;
                OnPickUp();
            }
        }

        private void Update()
        {
            if(Picked && stuckOnObject && joint && Input.GetKey(KeyCode.E))
            {
                t += Time.deltaTime / (timeToReachTarget);
                stuckOnObject.transform.position = Vector3.Lerp(stuckOnObject.transform.position, playerGrip.position, t);
            }
            else
            {
                OnRelease();
            }
        }

        public Rigidbody Rigid
        {
            get => rigid;
            set => rigid = value;
        }

        private bool Picked
        {
            get;
            set;
        }

        public void OnHold()
        {

        }

        public void OnPickUp()
        {
            Debug.Log("Picked Up " + gameObject.name);
            Picked = true;

            stuckOnObject = new GameObject();
            stuckOnObject.name = "StickPoint";
            stuckOnObject.AddComponent<SphereCollider>().isTrigger = true;
            stuckOnObject.AddComponent<Rigidbody>().isKinematic = true;

            stuckOnObject.transform.position = contactPoint;

            #region Joint Config
            joint = gameObject.AddComponent<ConfigurableJoint>();
            //gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
            joint.connectedBody = stuckOnObject.GetComponent<Rigidbody>();
            joint.autoConfigureConnectedAnchor = false;
            //joint.enableProjection = true;
            joint.anchor = transform.InverseTransformPoint(contactPoint);
            joint.axis = new Vector3(1f, 0, 0);
            joint.connectedAnchor = Vector3.zero;


            joint.yMotion = ConfigurableJointMotion.Locked;
            JointDrive jointDrive = new JointDrive();
            jointDrive.positionSpring = 50f;
            jointDrive.positionDamper = 1f;
            jointDrive.maximumForce = 10000f;

            joint.xDrive = jointDrive;
            joint.yDrive = jointDrive;
            joint.zDrive = jointDrive;
            #endregion

        }

        public void OnRelease()
        {
            Debug.Log("Released " + gameObject.name);
            Picked = false;

            if (joint != null)
            {
                Destroy(joint);
                Destroy(stuckOnObject);
            }

            this.playerGrip = null;
            this.contactPoint = Vector3.zero;
        }
    }
}
