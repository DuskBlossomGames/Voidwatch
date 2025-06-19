using System.Collections.Generic;
using UnityEngine;

namespace Util
{
    public static class InputManager
    {
        public static bool isPaused;

        private static Vector3 _oldMousePosition;
        public static Vector3 mousePosition
        {
            get
            {
                if (!isPaused) _oldMousePosition = Input.mousePosition;
                return isPaused ? _oldMousePosition : Input.mousePosition;
            }
        }
        
        private static Vector3 _oldMouseScrollDelta;
        public static Vector2 mouseScrollDelta
        {
            get
            {
                if (!isPaused) _oldMouseScrollDelta = Input.mouseScrollDelta;
                return isPaused ? _oldMouseScrollDelta : Input.mouseScrollDelta;
            }
        }

        public static bool GetKey(KeyCode key) { return !isPaused && Input.GetKey(key); }
        public static bool GetKeyDown(KeyCode key) { return !isPaused && Input.GetKeyDown(key); }
        public static bool GetKeyUp(KeyCode key) { return !isPaused && Input.GetKeyUp(key); }

        public static bool GetKey(InputAction action) { return GetKey(InputActions[action]); }
        public static bool GetKeyDown(InputAction action) { return GetKeyDown(InputActions[action]); }
        public static bool GetKeyUp(InputAction action) { return GetKeyUp(InputActions[action]); }

        
        public static readonly Dictionary<InputAction, KeyCode> InputActions = new();

        public static readonly Dictionary<InputAction, KeyCode> DEFAULT_ACTIONS = new()
        {
            { InputAction.Accelerate, KeyCode.W },
            { InputAction.Brake, KeyCode.S },
            { InputAction.Dash, KeyCode.Space },
            { InputAction.PrimaryWeapon, KeyCode.Mouse0 },
            { InputAction.SecondaryWeapon, KeyCode.Mouse1 }
        };
    }
    
    public enum InputAction
    {
        Accelerate = 0,
        Brake = 1,
        Dash = 2,
        PrimaryWeapon = 3,
        SecondaryWeapon = 4,
    }
}