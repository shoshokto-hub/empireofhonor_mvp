#if !UNITY_5_3_OR_NEWER
using System;
using UnityEngine;

namespace UnityEngine.InputSystem
{
    public class InputAction
    {
        public struct CallbackContext
        {
            public bool performed { get; set; }
        }

        public event Action<CallbackContext> performed;

        public void Enable()
        {
        }

        public void Disable()
        {
        }

        public bool IsPressed()
        {
            return false;
        }

        public T ReadValue<T>()
        {
            return default;
        }

        public void TriggerPerformed()
        {
            performed?.Invoke(new CallbackContext { performed = true });
        }
    }

    public class InputActionMap
    {
        public string name { get; set; }
    }

    public class PlayerInput : MonoBehaviour
    {
        public InputActionMap currentActionMap { get; private set; } = new InputActionMap();

        public void SwitchCurrentActionMap(string mapName)
        {
            currentActionMap = new InputActionMap { name = mapName };
        }
    }

    public class InputActionReference : ScriptableObject
    {
        [SerializeField] private InputAction actionInstance = new InputAction();

        public InputAction action => actionInstance;
    }

    public class InputControl<T>
    {
        public T ReadValue()
        {
            return default;
        }
    }

    public class Mouse
    {
        public static Mouse current => null;

        public InputControl<Vector2> position => new InputControl<Vector2>();
    }

    public class Touchscreen
    {
        public static Touchscreen current => null;

        public TouchControl primaryTouch => new TouchControl();
    }

    public class TouchControl
    {
        public InputControl<Vector2> position => new InputControl<Vector2>();
    }
}
#endif
