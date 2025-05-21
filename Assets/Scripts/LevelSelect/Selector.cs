using System;
using UnityEngine;

namespace Util
{
    public class Selector : MonoBehaviour
    {
        public float secondsToRotate;

        public event Action<Vector3?> OnSelectionChange;
        
        public Vector3 Position => transform.position;
        
        public void SetPosition(Vector3? position)
        {
            transform.position = position.GetValueOrDefault();
            gameObject.SetActive(position.HasValue);

            OnSelectionChange?.Invoke(position);
        }

        private void FixedUpdate()
        {
            transform.Rotate(Vector3.forward, 360 * Time.fixedDeltaTime / secondsToRotate);
        }
    }
}
