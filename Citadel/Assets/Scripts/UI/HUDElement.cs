using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDElement : MonoBehaviour
{
    public UIType uiType = UIType.Text;
    [HideInInspector]
    public float elementOpacity;

    public enum UIType { Text, Image };

    private Text textComponent;
    private Image imageComponent;
    private Color elementColor;

    // Start is called before the first frame update
    void Awake()
    {
        if(uiType == UIType.Text)
        {
            textComponent = GetComponent<Text>();
            elementColor = textComponent.color;
        }
        else if (uiType == UIType.Image)
        {
            imageComponent = GetComponent<Image>();
            elementColor = imageComponent.color;
        }
    }

    // Update is called once per frame
    void Update()
    {
        elementColor.a = elementOpacity;
        if (textComponent)
        {
            textComponent.color = elementColor;
        }
        else if (imageComponent)
        {
            imageComponent.color = elementColor;
        }
    }
}
