using LevelSelect;
using UnityEngine;

public class PlanetSetup : MonoBehaviour
{
    public MerchantData merchantData; // gotta keep it loaded
    
    public LevelSelectData data;
    public GameObject forceField;

    private void Awake()
    {
        var level = data.Levels[data.CurrentPlanet];

        GetComponent<SpriteRenderer>().sprite = level.Sprite;
        if (level.Type != LevelType.Normal && level.Type != LevelType.Elite)
        {
            forceField.SetActive(false);
        }
    }
}
