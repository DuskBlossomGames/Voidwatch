using System;
using UnityEngine;

namespace Menus
{
    public class CheckboxController : MonoBehaviour
    {
        public event Action<bool> OnToggle;

        private GameObject _check;

        public void Setup()
        {
            _check = transform.GetChild(0).gameObject;
        }

        public void SetValue(bool value) { _check.SetActive(value); }
        public void Toggle()
        {
            _check.SetActive(!_check.activeSelf);
            OnToggle?.Invoke(_check.activeSelf);
        }
    }
}