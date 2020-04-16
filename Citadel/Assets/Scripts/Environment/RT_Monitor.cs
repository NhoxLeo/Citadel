using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VHS;
using UnityEngine.Rendering.PostProcessing;

public class RT_Monitor : MonoBehaviour
{
    public GameObject renderTarget;
    public Camera renderCamera;
    public bool doRenderOnce = false;
    //public PostProcessLayer postProcessLayer;

    private RenderTexture renderTexture;
    private Material renderMat;

    // Start is called before the first frame update
    void Start()
    {
        renderCamera.enabled = false;
        StartCoroutine(Setup());
    }

    IEnumerator Setup()
    {
        yield return new WaitUntil(() => InteractionController.instance);

        renderTexture = new RenderTexture(320, 240, 8, RenderTextureFormat.ARGB32);

        renderCamera.targetTexture = renderTexture;

        Material currentMat = renderTarget.GetComponent<Renderer>().material;

        renderMat = new Material(currentMat.shader);
        renderMat.CopyPropertiesFromMaterial(currentMat);
        renderMat.mainTexture = renderTexture;

        renderTarget.GetComponent<Renderer>().material = renderMat;

        renderTexture.Create();
        renderCamera.enabled = true;

        if(doRenderOnce)
        {
            yield return new WaitForSeconds(1f);
            renderCamera.enabled = false;
        }
    }
}
