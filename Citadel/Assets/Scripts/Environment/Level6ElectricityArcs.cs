using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level6ElectricityArcs : MonoBehaviour
{
    public List<GameObject> arcObjects;
    public float MIN_EXIST_TIME = 0.1f, MAX_EXIST_TIME = 1f;

    private bool isActive = false;

    // Update is called once per frame
    void Update()
    {
        if (isActive)
        {
            StartCoroutine(FireLaser());
        }
    }

    private IEnumerator FireLaser()
    {
        int arcChosen = Random.Range(0, arcObjects.Count);

        if (!arcObjects[arcChosen].activeSelf)
        {
            arcObjects[arcChosen].SetActive(true);
            yield return new WaitForSeconds(Random.Range(MIN_EXIST_TIME, MAX_EXIST_TIME));
            arcObjects[arcChosen].SetActive(false);
        }
    }

    public void ActivateLasers()
    {
        isActive = true;
    }

    public void DeactivateLasers()
    {
        isActive = false;
    }
}
