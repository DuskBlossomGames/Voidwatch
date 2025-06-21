using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
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
        
        public static readonly Dictionary<KeyCode, string> VALID_KEY_CODES = new();
        static InputManager()
        {
            VALID_KEY_CODES.Add(KeyCode.Return, "Enter");
            VALID_KEY_CODES.Add(KeyCode.Space, "Space");
            VALID_KEY_CODES.Add(KeyCode.LeftShift, "LShift");
            VALID_KEY_CODES.Add(KeyCode.RightShift, "RShift");
            VALID_KEY_CODES.Add(KeyCode.UpArrow, "↑");
            VALID_KEY_CODES.Add(KeyCode.DownArrow, "↓");
            VALID_KEY_CODES.Add(KeyCode.RightArrow, "→");
            VALID_KEY_CODES.Add(KeyCode.LeftArrow, "←");
            VALID_KEY_CODES.Add(KeyCode.Mouse0, "LMB");
            VALID_KEY_CODES.Add(KeyCode.Mouse1, "RMB");
            for (var i = KeyCode.Exclaim; i <= KeyCode.Tilde; i++) VALID_KEY_CODES.Add(i, ("" + (char)i).ToUpper());
            for (var i = KeyCode.Mouse2; i <= KeyCode.Mouse6; i++) VALID_KEY_CODES.Add(i, $"MB{i-KeyCode.Mouse0}");
        }

        public static readonly Dictionary<int, KeyCode> InputActions = new();

        public static readonly Dictionary<int, KeyCode> DEFAULT_ACTIONS = new()
        {
            { InputAction.Accelerate, KeyCode.W },
            { InputAction.Brake, KeyCode.S },
            { InputAction.Dash, KeyCode.Space },
            { InputAction.PrimaryWeapon, KeyCode.Mouse0 },
            { InputAction.SecondaryWeapon, KeyCode.Mouse1 }
        };
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class InputAction
    {
        public static readonly InputAction Accelerate = new();
        public static readonly InputAction Brake = new();
        public static readonly InputAction Dash = new();
        public static readonly InputAction PrimaryWeapon = new();
        public static readonly InputAction SecondaryWeapon = new();

        public static int Count { get; private set; }
        
        public static InputAction Parse(string s)
        {
            return (InputAction)typeof(InputAction).GetField(s, BindingFlags.Static | BindingFlags.Public)!.GetValue(null);
        }

        private readonly int _idx;

        private InputAction() { _idx = Count++; }

        public static implicit operator InputAction(int idx) { return (InputAction) typeof(InputAction).GetFields(BindingFlags.Static | BindingFlags.Public)[..^1][idx].GetValue(null); }
        public static implicit operator int(InputAction action) { return action._idx; }
        public static implicit operator KeyCode(InputAction action) { return InputManager.InputActions[action]; }
    }
}