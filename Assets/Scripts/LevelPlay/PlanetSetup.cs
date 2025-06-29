using UnityEngine;
using static Singletons.Static_Info.LevelSelectData;
namespace LevelPlay
{
    public class PlanetSetup : MonoBehaviour
    {
        private void Awake()
        {
            var level = LevelSelectDataInstance.Levels[LevelSelectDataInstance.CurrentPlanet];

            GetComponent<SpriteRenderer>().sprite = level.Sprite;
        }
    }
}
