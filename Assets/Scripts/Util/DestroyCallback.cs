using System;
using UnityEngine;

namespace Util
{
    public class DestroyCallback : MonoBehaviour
    {
        public event Action Destroyed;

        private void OnDestroy()
        {
            Destroyed?.Invoke();
        }
    }
}