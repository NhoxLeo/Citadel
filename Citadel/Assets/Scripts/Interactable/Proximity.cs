using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Proximity : ActivationTrigger {

    private bool activated = false;
    public override bool Activated { get { return activated; } }
    private float progress = 0;
    public override float Progress { get { return progress; } }

    public bool Tracking = true;
    public Vector3 IdealPosition;
    public Vector3 IdealRotation;
    public Transform TargetObject;
    public Renderer GhostRenderer;
    public Color ValidColor;
    public Color InvalidColor;
    public float Tolerance;
    public float FloorTolerance;
    public float FloorHeight = .9f;

    private bool Trigger = false;

    public bool Active;
    private AudioSource AudioSource;
    private AudioClip AudioClip;

    private void Start()
    {
        AudioSource = GetComponent<AudioSource>();
        AudioClip = AudioSource.clip;
    }
    private void OnEnable()
    {
        StartCoroutine(DelaySoundTrigger());
    }
    private IEnumerator DelaySoundTrigger()
    {
        yield return new WaitForSeconds(.25f);
        Active = true;
    }

    private void OnDisable()
    {
        Active = false;
    }

    // Update is called once per frame
    void Update () {
        if(Tracking)
        {
            if (Vector3.Distance(IdealPosition, TargetObject.position) <= Tolerance && TargetObject.position.y <= FloorTolerance)
            {
                GhostRenderer.materials[0].SetColor("_TintColor", ValidColor);
                GhostRenderer.materials[1].SetColor("_TintColor", ValidColor);
                if(Trigger == false && Active)
                {
                    Trigger = true;
                    CorrectPlacement();
                }
            }
            else
            {
                GhostRenderer.materials[0].SetColor("_TintColor", InvalidColor);
                GhostRenderer.materials[1].SetColor("_TintColor", InvalidColor);
                if (Trigger == true)
                {
                    Trigger = false;
                }
            }
        }
	}

    public void CorrectPlacement()
    {
        if(Active)
        {
            AudioSource.PlayOneShot(AudioClip);
            Active = false;
            StartCoroutine(DelaySoundTrigger());
        }
    }

    private void LateUpdate() // ATTEMPT TO LOCK THE POSITION AND ROTATION
    {
        TargetObject.position = new Vector3(25.73f, TargetObject.position.y, TargetObject.position.z);
        TargetObject.eulerAngles = IdealRotation;

        if(TargetObject.position.y <= FloorHeight)
        {
            TargetObject.position = new Vector3(TargetObject.position.x, FloorHeight, TargetObject.position.z);
        }
    }
}
