using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using EmpireOfHonor.Gameplay;

#if ENABLE_INPUT_SYSTEM
#define INPUT_SYSTEM_ENABLED
#endif

#if INPUT_SYSTEM_ENABLED
using UnityEngine.InputSystem;
#endif

namespace EmpireOfHonor.Input
{
    /// <summary>
    /// Issues tactical orders to unit groups based on player input.
    /// </summary>
    public class CommandOverlay_Input : MonoBehaviour
    {
        [Serializable]
        private class UnitGroup
        {
            public string name;
            public List<UnitController> units = new();
        }

        [SerializeField] private Camera tacticalCamera;
        [SerializeField] private LayerMask groundMask = ~0;
        [SerializeField] private float navMeshSampleDistance = 5f;
        [SerializeField] private UnitGroup[] groups = new UnitGroup[4];

#if INPUT_SYSTEM_ENABLED
        [Header("Input Actions")]
        [SerializeField] private InputActionReference commandAction;
        [SerializeField] private InputActionReference holdAction;
        [SerializeField] private InputActionReference modifierAltAction;
        [SerializeField] private InputActionReference selectGroup1Action;
        [SerializeField] private InputActionReference selectGroup2Action;
        [SerializeField] private InputActionReference selectGroup3Action;
        [SerializeField] private InputActionReference selectGroup4Action;
#endif

        private bool tacticalMode;
        private int currentGroupIndex;

        private void OnEnable()
        {
#if INPUT_SYSTEM_ENABLED
            EnableAction(commandAction, HandleCommand);
            EnableAction(holdAction, HandleHold);
            EnableAction(selectGroup1Action, HandleSelectGroup1);
            EnableAction(selectGroup2Action, HandleSelectGroup2);
            EnableAction(selectGroup3Action, HandleSelectGroup3);
            EnableAction(selectGroup4Action, HandleSelectGroup4);
#else
            Debug.LogWarning(
                "CommandOverlay_Input requires the Unity Input System package. Please enable it in Project Settings > Player.");
#endif

            if (groups == null || groups.Length == 0)
            {
                groups = new UnitGroup[4];
            }
        }

        private void OnDisable()
        {
#if INPUT_SYSTEM_ENABLED
            DisableAction(commandAction, HandleCommand);
            DisableAction(holdAction, HandleHold);
            DisableAction(selectGroup1Action, HandleSelectGroup1);
            DisableAction(selectGroup2Action, HandleSelectGroup2);
            DisableAction(selectGroup3Action, HandleSelectGroup3);
            DisableAction(selectGroup4Action, HandleSelectGroup4);
#endif
        }

        /// <summary>
        /// Toggles whether tactical commands should be processed.
        /// </summary>
        public void SetTacticalMode(bool value)
        {
            tacticalMode = value;
        }

        private IReadOnlyList<UnitController> GetCurrentGroup()
        {
            if (groups == null || groups.Length == 0)
            {
                return Array.Empty<UnitController>();
            }

            if (currentGroupIndex < 0 || currentGroupIndex >= groups.Length || groups[currentGroupIndex] == null)
            {
                return Array.Empty<UnitController>();
            }

            return groups[currentGroupIndex].units;
        }

        private void SelectGroup(int index)
        {
            if (index < 0 || groups == null || index >= groups.Length)
            {
                return;
            }

            currentGroupIndex = index;
        }

#if INPUT_SYSTEM_ENABLED
        private void HandleSelectGroup1(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                SelectGroup(0);
            }
        }

        private void HandleSelectGroup2(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                SelectGroup(1);
            }
        }

        private void HandleSelectGroup3(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                SelectGroup(2);
            }
        }

        private void HandleSelectGroup4(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                SelectGroup(3);
            }
        }

        private void HandleCommand(InputAction.CallbackContext context)
        {
            if (!context.performed || !tacticalMode || !IsModifierActive())
            {
                return;
            }

            if (tacticalCamera == null)
            {
                return;
            }

            var ray = tacticalCamera.ScreenPointToRay(GetPointerPosition());
            if (!Physics.Raycast(ray, out var hitInfo, 500f, groundMask, QueryTriggerInteraction.Ignore))
            {
                return;
            }

            var targetHealth = hitInfo.collider.GetComponentInParent<Health>();
            if (targetHealth != null && targetHealth.TeamId == Health.Team.Player)
            {
                targetHealth = null;
            }

            if (targetHealth != null)
            {
                IssueAttack(targetHealth);
            }
            else
            {
                var destination = hitInfo.point;
                if (NavMesh.SamplePosition(destination, out var navHit, navMeshSampleDistance, NavMesh.AllAreas))
                {
                    destination = navHit.position;
                }

                IssueMove(destination);
            }
        }

        private void HandleHold(InputAction.CallbackContext context)
        {
            if (!context.performed || !tacticalMode || !IsModifierActive())
            {
                return;
            }

            foreach (var unit in GetCurrentGroup())
            {
                unit?.OrderHold();
            }
        }

        private void IssueMove(Vector3 destination)
        {
            foreach (var unit in GetCurrentGroup())
            {
                unit?.OrderMove(destination);
            }
        }

        private void IssueAttack(Health target)
        {
            foreach (var unit in GetCurrentGroup())
            {
                unit?.OrderAttack(target);
            }
        }

        private bool IsModifierActive()
        {
#if INPUT_SYSTEM_ENABLED
            return modifierAltAction != null && modifierAltAction.action.IsPressed();
#else
            return false;
#endif
        }

#if INPUT_SYSTEM_ENABLED
        private Vector2 GetPointerPosition()
        {
            if (Mouse.current != null)
            {
                return Mouse.current.position.ReadValue();
            }

            if (Touchscreen.current != null)
            {
                return Touchscreen.current.primaryTouch.position.ReadValue();
            }

            return new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        }

        private void EnableAction(InputActionReference actionReference, Action<InputAction.CallbackContext> handler)
        {
            if (actionReference == null || handler == null)
            {
                return;
            }

            actionReference.action.Enable();
            actionReference.action.performed += handler;
        }

        private void DisableAction(InputActionReference actionReference, Action<InputAction.CallbackContext> handler)
        {
            if (actionReference == null || handler == null)
            {
                return;
            }

            actionReference.action.performed -= handler;
            actionReference.action.Disable();
        }
#endif
    }
}
