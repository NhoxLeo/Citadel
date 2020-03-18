using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss2Lasers : MonoBehaviour
{
    public List<GameObject> towerPairs;
    public float MINDELAY = 5, MAXDELAY = 20;

    private bool isActive = false;
    private bool isCurrentlyUsing = false;

    // Update is called once per frame
    void Update()
    {
        if (isActive && !isCurrentlyUsing)
        {
            isCurrentlyUsing = true;
            StartCoroutine(FireLaser());
        }
    }

    private IEnumerator FireLaser()
    {
        yield return new WaitForSeconds(Random.Range(MINDELAY, MAXDELAY));
        int pairChosen = Random.Range(0, towerPairs.Count);

        Animator pairAnim = towerPairs[pairChosen].GetComponent<Animator>();
        pairAnim.SetTrigger("Fire");

        yield return new WaitForSeconds(1f);
        yield return new WaitUntil(() => (pairAnim.GetCurrentAnimatorStateInfo(0).IsName("Default")));

        StartCoroutine(FireLaser());
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
