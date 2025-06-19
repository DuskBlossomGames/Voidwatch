using UnityEngine;

namespace ProgressBars
{
    public class ProgressBar : MonoBehaviour
    {
        public virtual void UpdatePercentage(float cur, float max)
        {
            var underneath = transform.GetChild(transform.childCount - 1);
            underneath.localScale = new Vector3(2*(1 - Mathf.Clamp(cur, 0, max)/max), underneath.localScale.y, underneath.localScale.z);
        }
    }
}