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
        private SpringJoint joint;
        private Vector3 contactPoint;
        private LineRenderer lineRenderer;
        private Vector3 myGrabPoint, myHandPoint;
        private bool picked;

        private void Awake()
        {
            rigid = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            if(picked)
            {
                if (Input.GetKey(KeyCode.E))
                {
                    DrawGrabbing();
                    OnPickUp();
                }
                else
                {
                    OnRelease();
                }
            }
            else
            {
                if (lineRenderer)
                {
                    StartCoroutine(FadeLine(0, 1, true));
                }
            }
        }

        public override void OnInteract(Vector3 contactPoint, Transform playerGrip = null)
        {
            if(!picked)
            {
                this.playerGrip = playerGrip;
                this.contactPoint = contactPoint;
                OnPickUp();
            }
        }

        public Rigidbody Rigid
        {
            get => rigid;
            set => rigid = value;
        }

        public void OnHold()
        {
            joint.connectedAnchor = playerGrip.position;
            if (lineRenderer)
            {
                lineRenderer.widthCurve = AnimationCurve.Linear(0, 0.1f, 1, 0.1f);
            }

            if(rigid.velocity == Vector3.zero)
            {
                rigid.AddForce(new Vector3(0f, 0.000001f, 0f));
            }
        }

        private void StartGrab()
        {          
            joint = gameObject.AddComponent<SpringJoint>();
            joint.autoConfigureConnectedAnchor = false;
            joint.anchor = contactPoint;
            joint.minDistance = 0f;
            joint.maxDistance = 0f;
            joint.damper = 4f;
            joint.spring = 40f;
            joint.massScale = 5f;
            rigid.angularDrag = 5f;
            rigid.drag = 1f;

            lineRenderer = gameObject.AddComponent<LineRenderer>();
            lineRenderer = gameObject.GetComponent<LineRenderer>();
            lineRenderer.positionCount = 2;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.numCapVertices = 10;
            lineRenderer.numCornerVertices = 10;
            lineRenderer.widthCurve = AnimationCurve.Linear(0, 0.1f, 1, 0.1f);
            lineRenderer.startColor = new Color(255, 255, 255, 0);
            lineRenderer.endColor = new Color(255, 255, 255, 0);

            StopAllCoroutines();
            StartCoroutine(FadeLine(1, 1, false));
        }

        public void OnPickUp()
        {
            if (picked == false)
            {
                picked = true;
                StartGrab();
            }
            else if(joint)
            {
                OnHold();
            }
        }

        private void StopGrab()
        {
            Destroy(joint);
            StopAllCoroutines();
            if (lineRenderer)
            {
                StartCoroutine(FadeLine(0, 1, true));
            }

            rigid.angularDrag = 0.05f;
            rigid.drag = 0f;
            picked = false;
        }

        private void DrawGrabbing()
        {
            if (picked && lineRenderer)
            {
                myGrabPoint = Vector3.Lerp(myGrabPoint, transform.TransformPoint(contactPoint), Time.deltaTime * 45f);
                myHandPoint = Vector3.Lerp(myHandPoint, joint.connectedAnchor, Time.deltaTime * 45f);
                lineRenderer.SetPosition(0, myGrabPoint);
                lineRenderer.SetPosition(1, myHandPoint);
            }
        }

        public void OnRelease()
        {
            StopGrab();
     
            this.playerGrip = null;
            this.contactPoint = Vector3.zero;

            picked = false;
        }

        public IEnumerator FadeLine(float totalAlpha, float lerpDuration, bool destroy)
        {
            float t = 0;
            Color totalColor = Color.white;
            totalColor.a = totalAlpha;
         
            while (lineRenderer != null && (lineRenderer.startColor.a != totalAlpha))
            {
                t += Time.deltaTime / lerpDuration;
                lineRenderer.startColor = Color.Lerp(lineRenderer.startColor, totalColor, t);
                lineRenderer.endColor = lineRenderer.startColor;
                yield return null;
            }

            if (lineRenderer != null && (lineRenderer.startColor.a == totalAlpha))
            {
                if (destroy)
                {
                    Destroy(lineRenderer);
                }
            }
        }
    }
}
