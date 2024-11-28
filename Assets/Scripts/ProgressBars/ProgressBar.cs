using System;
using UnityEngine;

namespace ProgressBars
{
    public class ProgressBar : MonoBehaviour
    {
        private Transform _underneath;

        private void Start()
        {
            _underneath = transform.GetChild(transform.childCount-1);
        }

        public virtual void UpdatePercentage(float cur, float max)
        {
            _underneath.localScale = new Vector3(2*(1 - Mathf.Clamp(cur, 0, max)/max), _underneath.localScale.y, _underneath.localScale.z);
        }
    }
}