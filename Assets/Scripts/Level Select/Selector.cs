using System;
using UnityEngine;

namespace Level_Select
{
    public class Selector : MonoBehaviour
    {
        public float secondsToRotate;
        
        public void SetPosition(Vector3? position)
        {
            transform.position = position.GetValueOrDefault();
            gameObject.SetActive(position.HasValue);
        }

        private void FixedUpdate()
        {
            transform.Rotate(Vector3.forward, 360 * Time.fixedDeltaTime / secondsToRotate);
        }
    }
}
