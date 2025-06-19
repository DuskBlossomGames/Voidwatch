using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Util;

namespace Menus
{
    public class KeybindController : MonoBehaviour, IPointerDownHandler
    {
        public event Action<KeyCode> OnKeybindChange;
        
        private Button _button;
        private TextMeshProUGUI _keyText, _instructionText;

        public bool BindingKey { get; private set; }

        private static readonly Dictionary<KeyCode, string> ValidKeyCodes = new();

        static KeybindController()
        {
            ValidKeyCodes.Add(KeyCode.Return, "Enter");
            ValidKeyCodes.Add(KeyCode.Space, "Space");
            ValidKeyCodes.Add(KeyCode.LeftShift, "LShift");
            ValidKeyCodes.Add(KeyCode.RightShift, "RShift");
            ValidKeyCodes.Add(KeyCode.UpArrow, "↑");
            ValidKeyCodes.Add(KeyCode.DownArrow, "↓");
            ValidKeyCodes.Add(KeyCode.RightArrow, "→");
            ValidKeyCodes.Add(KeyCode.LeftArrow, "←");
            ValidKeyCodes.Add(KeyCode.Mouse0, "LMB");
            ValidKeyCodes.Add(KeyCode.Mouse1, "RMB");
            for (var i = KeyCode.Exclaim; i <= KeyCode.Tilde; i++) ValidKeyCodes.Add(i, ("" + (char)i).ToUpper());
            for (var i = KeyCode.Mouse2; i <= KeyCode.Mouse6; i++) ValidKeyCodes.Add(i, $"MB{i-KeyCode.Mouse0}");
        }

        public void Setup()
        {
            _button = GetComponent<Button>();
            _keyText = transform.GetComponentsInChildren<TextMeshProUGUI>(true)[0];
            _instructionText = transform.GetComponentsInChildren<TextMeshProUGUI>(true)[1];
        }

        private void LateUpdate()
        {
            _mouseDownWhileEditing &= Input.GetKey(KeyCode.Mouse0);
            if (!BindingKey) return;

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                BindingKey = false;
                _button.interactable = true;
                DisplayKey(KeyCode.None);

                OnKeybindChange?.Invoke(KeyCode.None);
            }

            foreach (var kvp in ValidKeyCodes)
            {
                if (!Input.GetKeyDown(kvp.Key)) continue;

                BindingKey = false;
                _button.interactable = true;
                DisplayKey(kvp.Key);
                OnKeybindChange?.Invoke(kvp.Key);
                
                break;
            }
        }

        private bool _mouseDownWhileEditing;
        public void OnPointerDown(PointerEventData evt) { _mouseDownWhileEditing = BindingKey; }

        public void DisplayKey(KeyCode key)
        {
            if (key == KeyCode.None)
            {
                _keyText.gameObject.SetActive(false);
                _instructionText.gameObject.SetActive(true);
                _instructionText.text = "<unbound>";
            }
            else
            {
                _keyText.gameObject.SetActive(true);
                _instructionText.gameObject.SetActive(false);
                _keyText.text = ValidKeyCodes[key];
            }
        }

        public void OnPress()
        {
            if (_mouseDownWhileEditing) return;
            
            BindingKey = true;
            _button.interactable = false;

            _keyText.gameObject.SetActive(false);
            _instructionText.gameObject.SetActive(true);
            _instructionText.text = "press";
        }
    }
}