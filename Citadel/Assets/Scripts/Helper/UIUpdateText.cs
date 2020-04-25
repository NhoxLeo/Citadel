using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIUpdateText : MonoBehaviour
{
    public Slider sliderToWatch;
    public string currentStringValue;

    public Text uiTextToUpdate;

    // Start is called before the first frame update
    void Start()
    {
        UpdateValue();
    }

    public void UpdateValue()
    {
        currentStringValue = ""+Mathf.Round(sliderToWatch.value);
        uiTextToUpdate.text = currentStringValue;
    }

    public string GetValue()
    {
        return currentStringValue;
    }
}
