using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIScaler : MonoBehaviour
{
    public Vector3 targetScale;
    public float scaleTime;

    private Vector3 startingScale;
    private bool scaling;

    // Start is called before the first frame update
    void Start()
    {
        startingScale = transform.localScale;
        StartCoroutine(ScaleToSize());
    }
    
    private IEnumerator ScaleToSize()
    {
        if (!scaling)
        {
            scaling = true;
            float t = 0;

            Vector3 targetVector;

            if(transform.localScale == startingScale)
            {
                targetVector = targetScale;
            }
            else
            {
                targetVector = startingScale;
            }

            while (transform.localScale != targetVector)
            {
                t += Time.deltaTime / scaleTime;
                transform.localScale = Vector3.Lerp(transform.localScale, targetVector, t);
                yield return null;
            }

            if (transform.localScale == targetVector)
            {
                scaling = false;
                StartCoroutine(ScaleToSize());
            }
        }
    }
}
