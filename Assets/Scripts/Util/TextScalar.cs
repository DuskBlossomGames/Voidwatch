using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextScalar : MonoBehaviour
{
    public int fontSizeAt1920;

    public int CurrFontSize()
    {
        float screenscale = (float)Camera.main.pixelWidth / 1920;
        screenscale = Mathf.Min(screenscale, (float)Camera.main.pixelHeight / 1080);
        return Mathf.RoundToInt(fontSizeAt1920 * screenscale);
    }
    void Start()
    {
        GetComponent<Text>().fontSize = Mathf.RoundToInt(fontSizeAt1920 * (float)Camera.main.pixelWidth / 1920);
    }

}
