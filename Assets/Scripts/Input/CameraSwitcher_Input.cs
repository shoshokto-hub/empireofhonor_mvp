using UnityEngine;
using UnityEngine.InputSystem;

namespace EmpireOfHonor.Input
{
    /// <summary>
    /// Switches between TPS and tactical cameras and toggles the appropriate action maps.
    /// </summary>
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
}
