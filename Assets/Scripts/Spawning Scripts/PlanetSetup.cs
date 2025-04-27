using LevelSelect;
using Static_Info;
using UnityEngine;

using static Static_Info.LevelSelectData;
public class PlanetSetup : MonoBehaviour
{
    public MerchantData merchantData; // gotta keep it loaded
    
    public GameObject forceField;

    private void Start()
    {
        var level = LevelSelectDataInstance.Levels[LevelSelectDataInstance.CurrentPlanet];

        GetComponent<SpriteRenderer>().sprite = level.Sprite;
        if (level.Type != LevelType.Normal && level.Type != LevelType.Elite)
        {
            forceField.SetActive(false);
        }
    }
}
