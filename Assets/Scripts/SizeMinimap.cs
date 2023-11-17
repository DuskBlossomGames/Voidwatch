using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class SizeMinimap : MonoBehaviour
{
    private void Awake()
    {
        var rect = (RectTransform)transform;
        var size = Screen.height * 0.4f;
        
        rect.sizeDelta = new Vector2(size, size);
        rect.anchoredPosition = new Vector2(-size/2, -size/2);
    }
}
