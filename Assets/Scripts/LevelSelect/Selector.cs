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
        
        private void FixedUpdate()
        {
            transform.Rotate(Vector3.forward, 360 * Time.fixedDeltaTime / secondsToRotate);
        }
    }
}
