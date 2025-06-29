using System.Reflection;
using TMPro;
using UnityEngine;

namespace Singletons.Static_Info
{
    public class Statistics : MonoBehaviour
    {
        private const float UNITS_TO_KM = 12756 / 22.5f; // based on planet ~radius of earth (12756km) which is 22.5 units in game
        
        public static Statistics StatisticsInstance => StaticInfoHolder.Instance.GetCachedComponent<Statistics>();

        // reflections needs these to be first
        public int levelsCleared;
        public int enemiesDefeated;
        public int scrapCollected;
        public int bulletsShot;
        public int timesDashed;
        public float distanceTraveled;
        // ------------------------------------

        public float startTime;
        

        public static void SetText(TextMeshProUGUI[] texts)
        {
            var fields = typeof(Statistics).GetFields(BindingFlags.Instance | BindingFlags.Public);
            for (var i = 0; i < texts.Length; i++)
            {
                var dt = fields[i].Name == "distanceTraveled";
                
                var val = fields[i].GetValue(StatisticsInstance);
                if (dt) val = (float) val / 1000 * UNITS_TO_KM;
                texts[i].text = $"{val:### ##0}" + (dt ? " km" : "");
            }
        }
    }
}