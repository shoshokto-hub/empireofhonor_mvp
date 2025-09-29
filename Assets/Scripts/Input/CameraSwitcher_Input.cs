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
    /// Switches between TPS and tactical cameras and toggles the appropriate action maps.
    /// </summary>
#if INPUT_SYSTEM_ENABLED
    [RequireComponent(typeof(PlayerInput))]
    public class CameraSwitcher_Input : MonoBehaviour
    {
        [SerializeField] private PlayerInput playerInput;
        [SerializeField] private Camera tpsCamera;
        [SerializeField] private Camera tacticalCamera;
        [SerializeField] private TPS_Input tpsController;
        [SerializeField] private Tactical_Input tacticalController;
        [SerializeField] private CommandOverlay_Input commandOverlay;
        [SerializeField] private InputActionReference switchAction;

        private bool tacticalMode;

        private void Awake()
        {
            playerInput ??= GetComponent<PlayerInput>();
        }

        private void OnEnable()
        {
            if (switchAction != null)
            {
                switchAction.action.Enable();
                switchAction.action.performed += HandleSwitch;
            }

            ApplyMode(false);
        }

        private void OnDisable()
        {
            if (switchAction != null)
            {
                switchAction.action.performed -= HandleSwitch;
                switchAction.action.Disable();
            }
        }

        private void HandleSwitch(InputAction.CallbackContext context)
        {
            if (!context.performed)
            {
                return;
            }

            ApplyMode(!tacticalMode);
        }

        private void ApplyMode(bool enableTactical)
        {
            tacticalMode = enableTactical;

            if (tpsCamera != null)
            {
                tpsCamera.gameObject.SetActive(!enableTactical);
            }

            if (tacticalCamera != null)
            {
                tacticalCamera.gameObject.SetActive(enableTactical);
            }

            if (tpsController != null)
            {
                tpsController.enabled = !enableTactical;
            }

            if (tacticalController != null)
            {
                tacticalController.enabled = enableTactical;
            }

            if (playerInput != null)
            {
                var targetMap = enableTactical ? "Tactical" : "Player";
                if (playerInput.currentActionMap == null || playerInput.currentActionMap.name != targetMap)
                {
                    playerInput.SwitchCurrentActionMap(targetMap);
                }
            }

            if (commandOverlay != null)
            {
                commandOverlay.SetTacticalMode(enableTactical);
            }

            if (enableTactical)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }
#else
    public class CameraSwitcher_Input : MonoBehaviour
    {
        [SerializeField] private Camera tpsCamera;
        [SerializeField] private Camera tacticalCamera;
        [SerializeField] private TPS_Input tpsController;
        [SerializeField] private Tactical_Input tacticalController;
        [SerializeField] private CommandOverlay_Input commandOverlay;

        private void Awake()
        {
            Debug.LogWarning(
                "CameraSwitcher_Input requires the Unity Input System package. Please enable it in Project Settings > Player.");
        }

        private void Start()
        {
            // Ensure the tactical overlay still knows about the default mode even without input switching.
            if (commandOverlay != null)
            {
                commandOverlay.SetTacticalMode(false);
            }

            if (tpsCamera != null)
            {
                tpsCamera.gameObject.SetActive(true);
            }

            if (tacticalCamera != null)
            {
                tacticalCamera.gameObject.SetActive(false);
            }

            if (tpsController != null)
            {
                tpsController.enabled = true;
            }

            if (tacticalController != null)
            {
                tacticalController.enabled = false;
            }

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
#endif
}
