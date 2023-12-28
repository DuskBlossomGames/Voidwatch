using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextScalar : MonoBehaviour
{
    public int fontSizeAt1920;

    public int CurrFontSize()
    {
        return Mathf.RoundToInt(fontSizeAt1920 * (float)Camera.main.pixelWidth / 1920);
    }
    void Start()
    {
        GetComponent<Text>().fontSize = Mathf.RoundToInt(fontSizeAt1920 * (float)Camera.main.pixelWidth / 1920);
    }

}
