using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VHS;

public class MapCamFollow : MonoBehaviour
{
    public Camera mapCam;
    public Camera FogOfWarCam;
    public Camera MapRendererCam;
    public GameObject player;

    private float startingY;
    private bool isSetUp;
    private bool firstTimeMoved;

    // Start is called before the first frame update
    void Start()
    {
        startingY = transform.position.y;
        StartCoroutine(Setup());
    }

    IEnumerator Setup()
    {
        yield return new WaitUntil(() => InteractionController.instance);
        player = InteractionController.instance.gameObject;
        isSetUp = true;
    }

    private void Update()
    {
        if (isSetUp && player)
        {
            UpdatePost();
        }
    }

    private void UpdatePost()
    {
        Vector3 newPos = player.transform.position;
        newPos.y = startingY;
        transform.position = newPos;
        transform.rotation = player.transform.rotation;

        if(!firstTimeMoved)
        {
            firstTimeMoved = true;
            MapRendererCam.enabled = false;
            FogOfWarCam.clearFlags = CameraClearFlags.Color;
            StartCoroutine(WaitToClear());
        }
    }

    private IEnumerator WaitToClear()
    {
        yield return new WaitUntil(() => FogOfWarCam.clearFlags == CameraClearFlags.Color);
        yield return new WaitForSeconds(0.2f);
        FogOfWarCam.clearFlags = CameraClearFlags.Depth;
    }
}

