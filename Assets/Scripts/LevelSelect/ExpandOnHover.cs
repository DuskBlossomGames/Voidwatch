using System;
using UnityEngine;

namespace Util
{
    public class ExpandOnHover : MonoBehaviour
    {
        public float secondsToExpand;
        public float expandScale;

        public float CurrentMultiplier { get; private set; } = 1;

        private int _direction;
        private void OnMouseEnter()
        {
            _direction = 1;
        }

        private void OnMouseExit()
        {
            _direction = -1;
        }

        private void FixedUpdate()
        {
            CurrentMultiplier = Math.Clamp(CurrentMultiplier + (expandScale-1) * _direction * Time.fixedDeltaTime / secondsToExpand, 1, expandScale);
        }
    }
}