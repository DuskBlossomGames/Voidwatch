using UnityEngine;
using Util.UI;
using static Singletons.Static_Info.LevelSelectData;
namespace LevelPlay
{
    public class PlanetSetup : MonoBehaviour
    {
        public static float Radius
        {
            get;
            private set;
        }
        
        private void Awake()
        {
            var level = LevelSelectDataInstance.Levels[LevelSelectDataInstance.CurrentPlanet];

            GetComponent<SpriteRenderer>().sprite = level.SpriteData.Sprite;

            transform.localScale *= level.SpriteData.RadiusMult;
            Radius = transform.localScale.x / 2;

            gameObject.AddComponent<PixelCollider2D>().Run(4); // compute collider
        }
    }
}
