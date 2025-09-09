using System;
using UnityEngine;

namespace Menus.Util
{
    public class CheckboxController : MonoBehaviour
    {
        public bool setupOnAwake;
        public event Action<bool> OnToggle;
        public bool Value => _check.activeSelf;

        private GameObject _check;

        private void Awake()
        {
            if (setupOnAwake) Setup();
        }

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