using UnityEngine;
using UnityEngine.UI;

namespace ProgressBars
{
    public class HUDBar : ProgressBar
    {
        private RectMask2D _mask;
        
        public void Awake()
        {
            _mask = transform.GetChild(transform.childCount-1).GetComponent<RectMask2D>();
        }
        
        public override void UpdatePercentage(float cur, float max)
        {
            if (!_mask) return;
            _mask.padding = new Vector4(0, 0, _mask.canvasRect.width * (1 - Mathf.Clamp(cur, 0, max) / max));
        }
    }
}