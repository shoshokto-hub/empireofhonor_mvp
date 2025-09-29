using UnityEngine;
using EmpireOfHonor.Gameplay;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace EmpireOfHonor.Input
{
    /// <summary>
    /// Handles third-person player input using the Unity Input System.
    /// </summary>
#if ENABLE_INPUT_SYSTEM
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(Weapon))]
    [RequireComponent(typeof(Health))]
    public class TPS_Input : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CharacterController characterController;
        [SerializeField] private Transform cameraRoot;
        [SerializeField] private Transform cameraPivot;

        [Header("Movement")]
        [SerializeField] private float moveSpeed = 4.5f;
        [SerializeField] private float sprintSpeed = 6.5f;
        [SerializeField] private float gravity = -9.81f;
        [SerializeField] private float jumpHeight = 1.5f;
        [SerializeField] private float rotationSpeed = 12f;

        [Header("Look")]
        [SerializeField] private float lookSensitivity = 120f;
        [SerializeField] private float minPitch = -45f;
        [SerializeField] private float maxPitch = 70f;

        [Header("Input Actions")]
        [SerializeField] private InputActionReference moveAction;
        [SerializeField] private InputActionReference lookAction;
        [SerializeField] private InputActionReference jumpAction;
        [SerializeField] private InputActionReference sprintAction;
        [SerializeField] private InputActionReference attackAction;

        private Vector3 velocity;
        private float pitch;
        private Weapon weapon;

        private void Awake()
        {
            characterController ??= GetComponent<CharacterController>();
            weapon = GetComponent<Weapon>();
        }

        private void OnEnable()
        {
            EnableAction(moveAction);
            EnableAction(lookAction);
            EnableAction(jumpAction);
            EnableAction(sprintAction);
            EnableAction(attackAction);

            if (attackAction != null)
            {
                attackAction.action.performed += HandleAttack;
            }

            if (jumpAction != null)
            {
                jumpAction.action.performed += HandleJump;
            }
        }

        private void OnDisable()
        {
            if (attackAction != null)
            {
                attackAction.action.performed -= HandleAttack;
            }

            if (jumpAction != null)
            {
                jumpAction.action.performed -= HandleJump;
            }

            DisableAction(moveAction);
            DisableAction(lookAction);
            DisableAction(jumpAction);
            DisableAction(sprintAction);
            DisableAction(attackAction);
        }

        private void Update()
        {
            HandleLook();
            HandleMovement();
        }

        private void HandleMovement()
        {
            if (moveAction == null || characterController == null)
            {
                return;
            }

            var moveInput = moveAction.action.ReadValue<Vector2>();
            var inputVector = new Vector3(moveInput.x, 0f, moveInput.y);
            var speed = sprintAction != null && sprintAction.action.IsPressed() ? sprintSpeed : moveSpeed;

            if (cameraRoot != null)
            {
                var forward = cameraRoot.forward;
                var right = cameraRoot.right;
                forward.y = 0f;
                right.y = 0f;
                forward.Normalize();
                right.Normalize();
                inputVector = forward * moveInput.y + right * moveInput.x;
            }

            if (inputVector.sqrMagnitude > 1f)
            {
                inputVector.Normalize();
            }

            var desiredVelocity = inputVector * speed;
            if (characterController.isGrounded && velocity.y < 0f)
            {
                velocity.y = -2f;
            }

            velocity.x = desiredVelocity.x;
            velocity.z = desiredVelocity.z;
            velocity.y += gravity * Time.deltaTime;

            characterController.Move(velocity * Time.deltaTime);

            if (inputVector.sqrMagnitude > 0.001f)
            {
                var targetRotation = Quaternion.LookRotation(inputVector.normalized, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }

        private void HandleLook()
        {
            if (lookAction == null || cameraPivot == null || cameraRoot == null)
            {
                return;
            }

            var lookInput = lookAction.action.ReadValue<Vector2>();
            var yawDelta = lookInput.x * lookSensitivity * Time.deltaTime;
            var pitchDelta = lookInput.y * lookSensitivity * Time.deltaTime;

            cameraRoot.Rotate(Vector3.up, yawDelta, Space.World);

            pitch = Mathf.Clamp(pitch - pitchDelta, minPitch, maxPitch);
            cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }

        private void HandleJump(InputAction.CallbackContext context)
        {
            if (!context.performed || characterController == null)
            {
                return;
            }

            if (characterController.isGrounded)
            {
                velocity.y = Mathf.Sqrt(-2f * gravity * jumpHeight);
            }
        }

        private void HandleAttack(InputAction.CallbackContext context)
        {
            if (!context.performed || weapon == null)
            {
                return;
            }

            weapon.TryAttack();
        }

        private static void EnableAction(InputActionReference reference)
        {
            reference?.action?.Enable();
        }

        private static void DisableAction(InputActionReference reference)
        {
            reference?.action?.Disable();
        }
    }
#else
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(Weapon))]
    [RequireComponent(typeof(Health))]
    public class TPS_Input : MonoBehaviour
    {
        private void Awake()
        {
            Debug.LogWarning(
                "TPS_Input requires the Unity Input System package. Please enable it in Project Settings > Player.");
        }
    }
#endif
}
