using UnityEngine;
using static Static_Info.LevelSelectData;
public class PlanetSetup : MonoBehaviour
{
    private void Start()
    {
        var level = LevelSelectDataInstance.Levels[LevelSelectDataInstance.CurrentPlanet];

        GetComponent<SpriteRenderer>().sprite = level.Sprite;
    }
}
