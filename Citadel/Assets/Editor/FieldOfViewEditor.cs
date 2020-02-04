using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor (typeof(FieldOfView))]
public class FieldOfViewEditor : Editor
{
    private void OnSceneGUI()
    {
        FieldOfView fov = (FieldOfView)target;
        Handles.color = Color.white;
        if (fov.aiController)
        {        
            Vector3 viewAngleA = fov.DirFromAngle(-fov.aiController.enemyParams.fov / 2, false);
            Vector3 viewAngleB = fov.DirFromAngle(fov.aiController.enemyParams.fov / 2, false);
            Handles.DrawWireArc((fov.transform.position + fov.transformOffset), Vector3.up, viewAngleA, fov.aiController.enemyParams.fov, fov.aiController.enemyParams.viewRange);

            Handles.DrawLine((fov.transform.position + fov.transformOffset), (fov.transform.position + fov.transformOffset) + viewAngleA * fov.aiController.enemyParams.viewRange);
            Handles.DrawLine((fov.transform.position + fov.transformOffset), (fov.transform.position + fov.transformOffset) + viewAngleB * fov.aiController.enemyParams.viewRange);

            Handles.color = Color.red;
            foreach (Transform visibleTarget in fov.visibleTargets)
            {
                Handles.DrawLine((fov.transform.position+fov.transformOffset), visibleTarget.position);
            }
        }
    }
}
