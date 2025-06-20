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

            foreach (var kvp in InputManager.VALID_KEY_CODES)
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
                _keyText.text = InputManager.VALID_KEY_CODES[key];
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