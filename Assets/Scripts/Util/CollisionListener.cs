using System;
using UnityEngine;

namespace Util
{
    public class CollisionListener : MonoBehaviour
    {
        public event Action<Collider2D> OnTriggerEnter;
        public event Action<Collider2D> OnTriggerStay;
        public event Action<Collider2D> OnTriggerExit;
        public event Action<Collision2D> OnCollisionEnter;
        public event Action<Collision2D> OnCollisionStay;
        public event Action<Collision2D> OnCollisionExit;


        private void OnTriggerEnter2D(Collider2D other) { OnTriggerEnter?.Invoke(other); }
        private void OnTriggerStay2D(Collider2D other) { OnTriggerStay?.Invoke(other); }
        private void OnTriggerExit2D(Collider2D other) { OnTriggerExit?.Invoke(other); }

        private void OnCollisionEnter2D(Collision2D other) { OnCollisionEnter?.Invoke(other); }
        private void OnCollisionStay2D(Collision2D other) { OnCollisionStay?.Invoke(other); }
        private void OnCollisionExit2D(Collision2D other) { OnCollisionExit?.Invoke(other); }
    }
}