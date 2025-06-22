using System.Reflection;
using TMPro;
using UnityEngine;

namespace Static_Info
{
    public class Statistics : MonoBehaviour
    {
        private const float UNITS_TO_KM = 12756 / 22.5f; // based on planet ~radius of earth (12756km) which is 22.5 units in game
        
        public static Statistics StatisticsInstance => StaticInfoHolder.Instance.GetCachedComponent<Statistics>();

        public int levelsCleared;
        public int enemiesDefeated;
        public int scrapCollected;
        public int bulletsShot;
        public int timesDashed;
        public float distanceTraveled;

        public static void SetText(TextMeshProUGUI[] texts)
        {
            var fields = typeof(Statistics).GetFields(BindingFlags.Instance | BindingFlags.Public);
            for (var i = 0; i < fields.Length; i++)
            {
                var val = fields[i].GetValue(StatisticsInstance);
                if (i == 0) val = (float) val / 1000 * UNITS_TO_KM;
                texts[i].text = $"{val:### ##0}" + (i == 0 ? " km" : "");
            }
        }
    }
}