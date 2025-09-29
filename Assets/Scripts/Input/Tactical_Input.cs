using UnityEngine;

#if ENABLE_INPUT_SYSTEM
#define INPUT_SYSTEM_ENABLED
#endif

#if INPUT_SYSTEM_ENABLED
using UnityEngine.InputSystem;
#endif

namespace EmpireOfHonor.Input
{
    /// <summary>
    /// Handles top-down tactical camera controls using the Unity Input System.
    /// </summary>
    public class Tactical_Input : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float panSpeed = 12f;
        [SerializeField] private float rotationSpeed = 90f;
        [SerializeField] private float zoomSpeed = 10f;
        [SerializeField] private float minHeight = 8f;
        [SerializeField] private float maxHeight = 40f;

#if INPUT_SYSTEM_ENABLED
        [Header("Input Actions")]
        [SerializeField] private InputActionReference panAction;
        [SerializeField] private InputActionReference rotateLeftAction;
        [SerializeField] private InputActionReference rotateRightAction;
        [SerializeField] private InputActionReference zoomAction;
#endif

        private Transform cachedTransform;

        private void Awake()
        {
            cachedTransform = transform;
        }

        private void OnEnable()
        {
#if INPUT_SYSTEM_ENABLED
            EnableAction(panAction);
            EnableAction(rotateLeftAction);
            EnableAction(rotateRightAction);
            EnableAction(zoomAction);
#else
            Debug.LogWarning(
                "Tactical_Input requires the Unity Input System package. Please enable it in Project Settings > Player.");
#endif
        }

        private void OnDisable()
        {
#if INPUT_SYSTEM_ENABLED
            DisableAction(panAction);
            DisableAction(rotateLeftAction);
            DisableAction(rotateRightAction);
            DisableAction(zoomAction);
#endif
        }

        private void Update()
        {
#if INPUT_SYSTEM_ENABLED
            HandlePan();
            HandleRotation();
            HandleZoom();
#endif
        }

#if INPUT_SYSTEM_ENABLED
        private void HandlePan()
        {
            if (panAction == null)
            {
                return;
            }

            var input = panAction.action.ReadValue<Vector2>();
            var forward = cachedTransform.forward;
            var right = cachedTransform.right;
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();

            var movement = (forward * input.y + right * input.x) * (panSpeed * Time.deltaTime);
            cachedTransform.position += movement;
        }

        private void HandleRotation()
        {
            float rotation = 0f;
            if (rotateLeftAction != null && rotateLeftAction.action.IsPressed())
            {
                rotation -= 1f;
            }

            if (rotateRightAction != null && rotateRightAction.action.IsPressed())
            {
                rotation += 1f;
            }

            if (Mathf.Abs(rotation) > 0.01f)
            {
                cachedTransform.Rotate(Vector3.up, rotation * rotationSpeed * Time.deltaTime, Space.World);
            }
        }

        private void HandleZoom()
        {
            if (zoomAction == null)
            {
                return;
            }

            var zoomValue = zoomAction.action.ReadValue<float>();
            if (Mathf.Abs(zoomValue) < 0.001f)
            {
                return;
            }

            var position = cachedTransform.position;
            position += cachedTransform.forward * (zoomValue * zoomSpeed * Time.deltaTime);
            position.y = Mathf.Clamp(position.y, minHeight, maxHeight);
            cachedTransform.position = position;
        }

        private static void EnableAction(InputActionReference reference)
        {
            reference?.action?.Enable();
        }

        private static void DisableAction(InputActionReference reference)
        {
            reference?.action?.Disable();
        }
#endif
    }
}
