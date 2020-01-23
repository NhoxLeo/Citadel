using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShadow : MonoBehaviour
{
    public Transform playerForward;
    private float defaultY, offsetX, offsetZ;

    // Start is called before the first frame update
    void Start()
    {
        transform.rotation = Quaternion.LookRotation(playerForward.transform.forward, transform.up);
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation = Quaternion.LookRotation(playerForward.transform.forward, transform.up);
    }
}
