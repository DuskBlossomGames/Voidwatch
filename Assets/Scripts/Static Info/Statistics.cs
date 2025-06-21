using System.Reflection;
using TMPro;
using UnityEngine;

namespace Static_Info
{
    public class Statistics : MonoBehaviour
    {
        public static Statistics StatisticsInstance => StaticInfoHolder.Instance.GetCachedComponent<Statistics>();

        public float distanceTraveled;
        public int bulletsShot;
        public int enemiesDefeated;
        public int scrapCollected;
        public int wavesCleared;
        public int levelsCleared;

        public static void SetText(TextMeshProUGUI[] texts)
        {
            var fields = typeof(Statistics).GetFields(BindingFlags.Instance | BindingFlags.Public);
            for (var i = 0; i < fields.Length; i++)
            {
                var val = fields[i].GetValue(StatisticsInstance);
                if (i == 0) val = (float) val / 1000;
                texts[i].text = $"{val:### ##0}" + (i == 0 ? " km" : "");
            }
        }
    }
}