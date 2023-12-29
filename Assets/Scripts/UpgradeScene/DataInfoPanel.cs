using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DataInfoPanel : MonoBehaviour
{
    public string objName;
    public string objDesc;

    public Text text;


    private void Update()
    {
        text.text = string.Format("{0}:\n{1}", objName, objDesc);
    }
}
