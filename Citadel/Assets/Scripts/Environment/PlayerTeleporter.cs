using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VHS;
using UnityEngine.Events;

public class PlayerTeleporter : MonoBehaviour
{
    public Transform teleportExit;
    public GameObject playerMesh;
    public UnityEvent OnTeleport;
    GameObject playerContainer;
    bool wasActivated = false;

    // Start is called before the first frame update
    void Start()
    {
        playerContainer = InteractionController.instance.transform.parent.gameObject;
    }

    public void Teleport()
    {
        if (!wasActivated)
        {
            wasActivated = true;
            OnTeleport.Invoke();
            playerContainer.transform.position = teleportExit.position;
            StartCoroutine(ForceTeleport());
        }
    }

    public IEnumerator ForceTeleport()
    {
        bool hasTeleported = false;
        while (!hasTeleported)
        {
            if(Vector3.Distance(InteractionController.instance.gameObject.transform.localPosition,Vector3.zero) < 1f)
            {
                InteractionController.instance.transform.GetChild(0).GetComponent<CameraController>().RotatePlayer(teleportExit.rotation.eulerAngles);
                hasTeleported = true;
                wasActivated = false;
            }
            else
            {
                InteractionController.instance.gameObject.transform.localPosition  = Vector3.zero;
            }

            yield return null;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(teleportExit.position, teleportExit.position + teleportExit.forward);
        if (playerMesh)
        {
            Gizmos.DrawMesh(playerMesh.transform.GetChild(1).GetComponent<MeshFilter>().sharedMesh, teleportExit.position);
        }
    }
}
