using System;
using UnityEngine;

namespace LevelSelect
{
    public class Selector : MonoBehaviour
    {
        public float secondsToRotate;

        public event Action<Vector3?> OnSelectionChange;
        
        public Vector3 Position => transform.position;

        private Vector3? _origScale;
        public void SetScaleMult(float scaleMult)
        {
            _origScale ??= transform.localScale;
            transform.localScale = _origScale.Value * scaleMult;
        }
        
        public void SetPosition(Vector3? position)
        {
            transform.position = position.GetValueOrDefault();
            gameObject.SetActive(position.HasValue);

            SetScaleMult(1);
            OnSelectionChange?.Invoke(position);
        }

        public void SetUsable(bool usable)
        {
            foreach (var sr in GetComponentsInChildren<SpriteRenderer>())
            {
                Color.RGBToHSV(sr.color, out var h, out var s, out _);
                sr.color = Color.HSVToRGB(h, s, usable ? 0.7f : 0.4f);
            }
        }
        
        private void FixedUpdate()
        {
            transform.Rotate(Vector3.forward, 360 * Time.fixedDeltaTime / secondsToRotate);
        }
    }
}
