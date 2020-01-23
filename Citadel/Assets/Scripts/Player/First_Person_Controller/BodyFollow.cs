using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VHS;
using NaughtyAttributes;

public class BodyFollow : MonoBehaviour
{
    public MovementInputData movementInputData;
    public FirstPersonController fpsController;
    public Transform bodyToFollow;
    public Vector3 followOffset;

    public Vector3 crouchScaleHeight;

    private Vector3 defaultStandingScale;
    private Vector3 defaultFollowOffset;
    private bool isCrouched;

    // Start is called before the first frame update
    void Start()
    {
        transform.position = bodyToFollow.position + followOffset;
        defaultStandingScale = transform.localScale;
        defaultFollowOffset = followOffset;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = bodyToFollow.position + followOffset;

        if (movementInputData.IsCrouching && !isCrouched)
        {
            isCrouched = true;
            StartCoroutine(HandleCrouch(isCrouched));
        }
        else if (!movementInputData.IsCrouching && isCrouched)
        {
            isCrouched = false;
            StartCoroutine(HandleCrouch(isCrouched));
        }
    }

    public IEnumerator HandleCrouch(bool toggle)
    {
        yield return new WaitForSeconds(fpsController.crouchTransitionDuration/2);
        if (toggle)
        {
            transform.localScale = crouchScaleHeight;
            followOffset = new Vector3(defaultFollowOffset.x, defaultFollowOffset.y / 2, defaultFollowOffset.z);
            transform.position = bodyToFollow.position + followOffset;
        }
        else
        {
            transform.localScale = defaultStandingScale;
            followOffset = defaultFollowOffset;
            transform.position = bodyToFollow.position + followOffset;
        }
    }
}
