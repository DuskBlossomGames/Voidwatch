using System;
using UnityEngine;

namespace Util
{
    public class Button : MonoBehaviour
    {
        public event Action OnClick;
        
        private void OnMouseUpAsButton()
        {
            OnClick?.Invoke();
        }
    }
}