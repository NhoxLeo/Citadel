using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VHS;

public class GrabIndicator : MonoBehaviour
{
    public InteractableBase thisInteractable;
    //FOR GRAB INDICATOR
    public Transform GrabSprite;
    [HideInInspector]
    private SpriteRenderer SpriteRenderer;
    private Vector3 initialScale;
    //private bool Trigger = false;
    private GameObject MyAvatar;
    private float Distance;
    private float MinDistance = 4f;
    public bool Grabbed = false;
    private float maxHitForce = 500;
    private GameObject collisionPing;
    private bool collisionActive = false;
    private bool isGrown;
    private bool Trigger = false;

    private void Awake()
    {
        //For Grab Indicator
        if (GrabSprite)
        {
            SpriteRenderer = GrabSprite.GetComponent<SpriteRenderer>();
            initialScale = GrabSprite.localScale;
        }
    }

    private void Start()
    {
        if(thisInteractable && InteractionController.instance)
        {
            MyAvatar = InteractionController.instance.gameObject;
        }
    }

    /*
    void OnCollisionEnter(Collision col)
    {
        Vector3 impactForce = col.impulse / Time.fixedDeltaTime;
        //Debug.Log(gameObject.name + " Impacted With " + col.gameObject.name + " With A Force Of " + impactForce.magnitude);   

        float impactVolume = 0.0f;

        if (impactForce.magnitude > 0.05)
        {
            impactVolume = (impactForce.magnitude / maxHitForce);
            if (impactVolume > 1.0)
            {
                impactVolume = 1.0f;
            }

            
            if (collisionPing)
            {
                GameObject collideSound = Instantiate(collisionPing, col.contacts[0].point, Quaternion.identity) as GameObject;
                collideSound.GetComponent<AudioSource>().volume = impactVolume;

                //Debug.Log(gameObject.name + " Impacted With " + col.gameObject.name + " With A Force Of " + impactForce.magnitude + " | Impact Volume Was " + impactVolume);
            }

        }
    }
    */

    private void Update()
    {
        if (MyAvatar && GrabSprite) //Checks how far away the player is and sets the active state of the indicator
        {
            Distance = Vector3.Distance(transform.position, MyAvatar.transform.position);
            if (!GrabSprite.gameObject.activeSelf && Distance <= MinDistance && !Grabbed)
            {
                GrabSprite.gameObject.SetActive(true);
            }
            else if (GrabSprite.gameObject.activeSelf && Distance > MinDistance)
            {
                GrabSprite.gameObject.SetActive(false);
            }

            if (InteractionController.instance.interactionData.Interactable)
            {
                if (InteractionController.instance.interactionData.Interactable == thisInteractable)
                {
                    isGrown = true;
                    GrowIndicator();
                }
                else if(InteractionController.instance.interactionData.Interactable != thisInteractable && isGrown)
                {
                    isGrown = false;
                    ShrinkIndicator();
                }
            }

        }  
    }

    public void GrowIndicator()
    {
        if (this.gameObject.activeSelf)
            StartCoroutine(GrabIndicatorAnim(true));
        //Trigger = true;
    }

    public void ShrinkIndicator()
    {
        if (GrabSprite.gameObject)
        {
            if (this.gameObject.activeSelf && GrabSprite.gameObject.activeSelf)
            {
                StartCoroutine(GrabIndicatorAnim(false));
            }
        }
        //Trigger = true;
    }

    public void GrabToggle(bool Toggle)
    {
        GrabSprite.gameObject.SetActive(!Toggle);
        Grabbed = Toggle;
    }

    private IEnumerator GrabIndicatorAnim(bool Toggle)
    {
        if (GrabSprite.gameObject.activeSelf)
        {
            Vector3 to;
            Color toColor;
            if (Toggle)
            {
                to = new Vector3(initialScale.x * 2f, initialScale.y * 2f, 1f);
                toColor = Color.yellow;
            }
            else
            {
                to = initialScale;
                toColor = Color.white;
            }



            Trigger = true;
            yield return new WaitForEndOfFrame();
            Trigger = false;
            float elapsed = 0.0f;
            while (elapsed < .2f && !Trigger)
            {
                GrabSprite.localScale = Vector3.Lerp(GrabSprite.localScale, to, elapsed / .2f);
                SpriteRenderer.color = Color.Lerp(SpriteRenderer.color, toColor, elapsed / .2f);
                elapsed += Time.deltaTime;
                yield return null;
            }

            if (Grabbed)
            {
                GrabSprite.localScale = initialScale;
                SpriteRenderer.color = Color.white;
            }
            else
            {
                GrabSprite.localScale = to;
                SpriteRenderer.color = toColor;
            }
        }
    }
}
