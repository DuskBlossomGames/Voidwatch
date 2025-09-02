using UnityEngine;
using static Singletons.Static_Info.LevelSelectData;

namespace Spawnables.Controllers.Bifurcator
{
    public class LazerSpawn : MonoBehaviour
    {
        public GameObject lazer;
        public GameObject innerCap;
        public GameObject outerCap;
        public GameObject body;
        public int innerRad;
        public int outerRad;
        // Start is called before the first frame update
        void Start()
        {
            transform.position = -Vector3.forward;

            var origY = innerCap.transform.localPosition.y;
            
            var radMult = LevelSelectDataInstance.Levels[LevelSelectDataInstance.CurrentPlanet].SpriteData.RadiusMult;
            innerCap.transform.localPosition *= radMult;
            lazer.transform.localPosition *= radMult;

            innerRad = (int)innerCap.transform.localPosition.y;
            outerRad = (int)outerCap.transform.localPosition.y;

            body.transform.localPosition = Vector3.up * (innerRad + (outerRad - innerRad) / 2f) - Vector3.forward;

            for (int i = 1; i < (outerRad-innerRad)/2; i++)
            {
                var nLazer = Instantiate(lazer, transform);
                nLazer.transform.localPosition = new Vector3(0, innerRad + 2 * i + 1, 0);
                nLazer.name = string.Format("Lazer_Segment_#{0}", i + 1);
            }
            //Destroy(lazer);
        }
    }
}
