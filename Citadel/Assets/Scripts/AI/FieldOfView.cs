using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldOfView : MonoBehaviour
{
    [Header("FOV References")]
    [Space(10)]
    public AIController aiController;
    public Vector3 transformOffset;

    [Header("FOV Masks")]
    [Space(10)]
    public LayerMask targetMask;
    public LayerMask obstacleMask;

    [Header("FOV Debug")]
    [Space(10)]
    public List<Transform> visibleTargets = new List<Transform>();

    private void Start()
    {
        StartCoroutine(FindTargetsWithDelay(.2f));
    }


    IEnumerator FindTargetsWithDelay(float delay)
    {
        while(true)
        {
            yield return new WaitForSeconds(delay);
            FindVisibleTargets();
        }
    }

    private void FindVisibleTargets()
    {
        visibleTargets.Clear();
        Collider[] targetsInViewRadius = Physics.OverlapSphere((transform.position + transformOffset), aiController.enemyParams.viewRange, targetMask);

        for(int i = 0; i < targetsInViewRadius.Length; i++)
        {
            Transform target = targetsInViewRadius[i].transform;
            Vector3 dirToTarget = (target.position - (transform.position+transformOffset)).normalized;

            if(Vector3.Angle(transform.forward, dirToTarget) < aiController.enemyParams.fov / 2)
            {
                float distanceToTarget = Vector3.Distance((transform.position + transformOffset), target.position);

                if(!Physics.Raycast((transform.position + transformOffset), dirToTarget, distanceToTarget, obstacleMask))
                {
                    visibleTargets.Add(target);
                }
            }
        }
    }

    public Vector3 DirFromAngle(float angleInDegress, bool angleIsGlobal)
    {
        if(!angleIsGlobal)
        {
            angleInDegress += transform.eulerAngles.y;
        }
        return new Vector3(Mathf.Sin(angleInDegress * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegress * Mathf.Deg2Rad));
    }
}
