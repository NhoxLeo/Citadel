using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    public bool lockXRotation, lockYRotation, lockZRotation;

    Quaternion originalRotation;

    void OnEnable()
    {
        transform.parent.rotation = Quaternion.identity;
        originalRotation = transform.rotation;
    }

    void Update()
    {
        Quaternion newRotation = new Quaternion();
        Vector3 lockRotation = Vector3.one;

        if(lockXRotation)
        {
            lockRotation.x = 0;
        }

        if (lockYRotation)
        {
            lockRotation.y = 0;
        }

        if (lockZRotation)
        {
            lockRotation.z = 0;
        }

        newRotation = Quaternion.Euler(Camera.main.transform.rotation.eulerAngles.x * lockRotation.x, Camera.main.transform.rotation.eulerAngles.y * lockRotation.y, Camera.main.transform.rotation.eulerAngles.z * lockRotation.z);

        transform.rotation = newRotation * originalRotation;
    }
}
