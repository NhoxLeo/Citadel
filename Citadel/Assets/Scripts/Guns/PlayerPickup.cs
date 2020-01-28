using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VHS;

public class PlayerPickup : MonoBehaviour
{
    public int currentPickUpIndex = 0;
    public AudioClip pickUpSound;
    public List<Pickup> pickUps;

    private GameObject player;

    // Start is called before the first frame update
    void Start()
    {
        if(pickUps == null)
        {
            pickUps = new List<Pickup>();
        }

        UpdateCurrentVisible();
    }

    private void OnTriggerEnter(Collider col)
    {
        if(col.gameObject.layer == 8) //Player
        {
            Pickup();
        }
    }

    private void Pickup()
    {
        if(pickUps[currentPickUpIndex].pickUpType == global::Pickup.PickUpType.Weapon)
        {
            InteractionController.instance.AddWeapon(pickUps[currentPickUpIndex].pickUpPrefab);
        }

        GameVars.instance.audioManager.PlaySFX(pickUpSound, 0.5f, transform.position);
        GameVars.instance.audioManager.PlaySFX(pickUps[currentPickUpIndex].pickUpSound, 0.5f, transform.position);

        Destroy(gameObject);
    }

    private void UpdateCurrentVisible()
    {
        if (pickUps != null && pickUps.Count > 0)
        {
            for (int i = 0; i < pickUps.Count; i++)
            {
                if (pickUps[i].pickUpCanvasIcon)
                {
                    if (i != currentPickUpIndex)
                    {
                        if (pickUps[i].pickUpCanvasIcon.activeSelf)
                        {
                            pickUps[i].pickUpCanvasIcon.SetActive(false);
                        }
                    }
                    else
                    {
                        if (!pickUps[i].pickUpCanvasIcon.activeSelf)
                        {
                            pickUps[i].pickUpCanvasIcon.SetActive(true);
                        }
                    }
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        UpdateCurrentVisible();
    }
}

[System.Serializable]
public class Pickup
{
    #region Variables & Inspector Options
    public PickUpType pickUpType = PickUpType.Weapon;
    public GameObject pickUpPrefab;
    public AudioClip pickUpSound;
    public GameObject pickUpCanvasIcon;

    #region Stored Data
    public enum PickUpType { Weapon, Item}
    #endregion
    #endregion
}
