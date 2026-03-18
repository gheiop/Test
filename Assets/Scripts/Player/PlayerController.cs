using UnityEngine;
using Islebound.Items;
using Islebound.UI;
using Islebound.Crafting;

namespace Islebound.Player
{
    [RequireComponent(typeof(PlayerInputReader))]
    [RequireComponent(typeof(PlayerMotor))]
    [RequireComponent(typeof(PlayerStats))]
    [RequireComponent(typeof(PlayerInteractor))]
    [RequireComponent(typeof(PlayerLook))]
    public class PlayerController : MonoBehaviour
    {
        private PlayerInputReader inputReader;
        private PlayerMotor motor;
        private PlayerStats stats;
        private PlayerInteractor interactor;
        private PlayerLook playerLook;

        private void Awake()
        {
            inputReader = GetComponent<PlayerInputReader>();
            motor = GetComponent<PlayerMotor>();
            stats = GetComponent<PlayerStats>();
            interactor = GetComponent<PlayerInteractor>();
            playerLook = GetComponent<PlayerLook>();
        }

        private void Update()
        {
            if (IsAnyGameplayBlockingUIOpen())
            {
                HandleUIOnlyInput();
                return;
            }

            HandleLook();
            HandleJump();
            HandleMovement();
            HandleInteraction();
            HandleHotbarSelection();
        }

        private bool IsAnyGameplayBlockingUIOpen()
        {
            bool inventoryOpen = InventoryManager.Instance != null && InventoryManager.Instance.IsInventoryOpen;
            bool workbenchOpen = WorkbenchUI.Instance != null && WorkbenchUI.Instance.IsOpen;
            return inventoryOpen || workbenchOpen;
        }

        private void HandleUIOnlyInput()
        {
            // Ничего не делаем специально:
            // инвентарь и верстак сами обрабатывают свои клавиши открытия/закрытия.
        }

        private void HandleLook()
        {
            playerLook.Look(inputReader.LookValue);
        }

        private void HandleMovement()
        {
            bool hasMoveInput = inputReader.MoveValue.sqrMagnitude > 0.01f;
            bool wantsSprint = inputReader.SprintHeld && hasMoveInput;
            bool canSprint = wantsSprint && stats.CanSprint();

            motor.Move(inputReader.MoveValue, transform, canSprint);

            if (canSprint)
                stats.DrainSprintStamina(Time.deltaTime);
            else
                stats.RecoverStamina(Time.deltaTime);
        }

        private void HandleJump()
        {
            if (!inputReader.WasJumpPressedThisFrame())
                return;

            if (!stats.TryConsumeJumpStamina())
                return;

            bool jumped = motor.TryJump();
            if (!jumped)
            {
                stats.SetStamina(stats.CurrentStamina + stats.JumpStaminaCost);
            }
        }

        private void HandleInteraction()
        {
            if (inputReader.WasInteractPressedThisFrame())
            {
                interactor.TryInteract();
            }
        }

        private void HandleHotbarSelection()
        {
            if (InventoryManager.Instance == null)
                return;

            float scrollValue = inputReader.ReadScrollValue();

            if (scrollValue > 0.01f)
                InventoryManager.Instance.ScrollSelection(1);
            else if (scrollValue < -0.01f)
                InventoryManager.Instance.ScrollSelection(-1);

            int hotbarIndex = inputReader.ReadHotbarNumberPressed();
            if (hotbarIndex >= 0 && hotbarIndex < InventoryManager.Instance.HotbarSize)
            {
                InventoryManager.Instance.SelectHotbarIndex(hotbarIndex);
            }
        }
    }
}