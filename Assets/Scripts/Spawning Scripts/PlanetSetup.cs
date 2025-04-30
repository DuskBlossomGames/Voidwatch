using LevelSelect;
using Static_Info;
using UnityEngine;

using static Static_Info.LevelSelectData;
public class PlanetSetup : MonoBehaviour
{
    public MerchantData merchantData; // gotta keep it loaded
    
    private void Start()
    {
        var level = LevelSelectDataInstance.Levels[LevelSelectDataInstance.CurrentPlanet];

        GetComponent<SpriteRenderer>().sprite = level.Sprite;
    }
}
