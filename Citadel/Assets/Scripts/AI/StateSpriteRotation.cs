using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VHS;

public class StateSpriteRotation : MonoBehaviour
{
    public GameObject spriteForward;
    public GameObject spriteForwardLeft;
    public GameObject spriteLeft;
    public GameObject spriteBackLeft;
    public GameObject spriteBack;
    public GameObject spriteBackRight;
    public GameObject spriteRight;
    public GameObject spriteForwardRight;

    private GameObject currentActiveSprite;
    private bool switchingSprites;
    private GameObject rotationReference;

    // Start is called before the first frame update
    void Start()
    {
        rotationReference = InteractionController.instance.gameObject;
        currentActiveSprite = spriteForward;
    }

    // Update is called once per frame
    void Update()
    {
        SwitchSprite(GetAngle());
        if(currentActiveSprite && !switchingSprites)
        {
            if(!currentActiveSprite.activeSelf)
            {
                currentActiveSprite.SetActive(true);
            }
        }
    }

    private float GetAngle()
    {
        var camPos = new Vector2(rotationReference.transform.forward.x, rotationReference.transform.forward.z);
        var parent = new Vector2(transform.parent.forward.x, transform.parent.forward.z);
        float enemyAngle = Vector2.Angle(camPos, parent);
        Vector3 cross = Vector3.Cross(camPos, parent);

        if (cross.z < 0)
        {
            enemyAngle = enemyAngle*-1;
        }
        return enemyAngle;
    }

    private void SwitchSprite(float angle)
    {
        switchingSprites = true;
        GameObject newSprite = null;

        if (angle > -22.5 && angle <= 22.5)
        {
            newSprite = spriteForward;
        }
        else if (angle > 22.5 && angle <= 67.5)
        {
            newSprite = spriteForwardLeft;
        }
        else if (angle > 67.5 && angle <= 112.5)
        {
            newSprite = spriteLeft;
        }
        else if (angle > 112.5 && angle <= 157.5)
        {
            newSprite = spriteBackLeft;
        }
        else if ((angle > 157.5 && angle <= 180) || (angle >= -180 && angle <= -157.5))
        {
            newSprite = spriteBack;
        }
        else if (angle > -157.5 && angle <= -112.5)
        {
            newSprite = spriteBackRight;
        }
        else if (angle > -112.5 && angle <= -67.5)
        {
            newSprite = spriteRight;
        }
        else if (angle > -67.5 && angle <= -22.5)
        {
            newSprite = spriteForwardRight;
        }

        if(newSprite != currentActiveSprite && newSprite != null)
        {
            SpriteSheet oldSpriteSheet = currentActiveSprite.GetComponent<SpriteSheet>();
            SpriteSheet newSpriteSheet = newSprite.GetComponent<SpriteSheet>();

            if(oldSpriteSheet && newSpriteSheet)
            {
                if(oldSpriteSheet.frames.Length == newSpriteSheet.frames.Length)
                {
                    newSpriteSheet.startingFrame = oldSpriteSheet.currentFrame;
                }
            }
            currentActiveSprite.SetActive(false);
            currentActiveSprite = newSprite;
        }

        switchingSprites = false;
    }
}
