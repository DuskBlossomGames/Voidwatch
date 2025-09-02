using UnityEngine;
using UnityEngine.UI;

namespace ProgressBars
{
    public class SimpleHUDBar : ProgressBar
    {
        public override void UpdatePercentage(float cur, float max)
        {
            var underneath = transform.GetChild(0) as RectTransform;

            underneath!.anchorMin = new Vector2(Mathf.Clamp(cur, 0, max)/max, 0);
        }
    }
}