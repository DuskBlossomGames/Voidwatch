using System;
using System.Collections;
using System.Collections.Generic;
using LevelSelect;
using UnityEngine;

public class PlanetSetup : MonoBehaviour
{
    public LevelSelectData data;
    public GameObject forceField;

    private void Awake()
    {
        var level = data.Levels[data.CurrentPlanet];

        GetComponent<SpriteRenderer>().sprite = level.Sprite;
        if (level.Type != LevelType.Normal) forceField.SetActive(false);
    }
}
