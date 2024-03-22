using System;
using UnityEngine;

namespace Util
{
    public class DestroyAfter : MonoBehaviour
    {
        public float timeToLive;

        private readonly Timer _timer = new();

        private void Start()
        {
            _timer.Value = timeToLive;
        }

        private void Update()
        {
            _timer.Update();
            if (_timer.IsFinished) Destroy(gameObject);
        }
    }
}