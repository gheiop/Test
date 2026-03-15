using UnityEngine;
using UnityEngine.InputSystem;

namespace Islebound.Player
{
    public class PlayerInputReader : MonoBehaviour
    {
        [Header("Input Actions")]
        [SerializeField] private InputActionAsset inputActions;

        private InputActionMap playerMap;
        private InputAction moveAction;
        private InputAction lookAction;
        private InputAction jumpAction;
        private InputAction sprintAction;
        private InputAction interactAction;
        private InputAction scrollHotbarAction;
        private InputAction[] hotbarActions;

        public Vector2 MoveValue => moveAction != null ? moveAction.ReadValue<Vector2>() : Vector2.zero;
        public Vector2 LookValue => lookAction != null ? lookAction.ReadValue<Vector2>() : Vector2.zero;
        public bool SprintHeld => sprintAction != null && sprintAction.IsPressed();

        private void Awake()
        {
            playerMap = inputActions.FindActionMap("Player", true);

            moveAction = playerMap.FindAction("Move", true);
            lookAction = playerMap.FindAction("Look", true);
            jumpAction = playerMap.FindAction("Jump", true);
            sprintAction = playerMap.FindAction("Sprint", true);
            interactAction = playerMap.FindAction("Interact", true);
            scrollHotbarAction = playerMap.FindAction("ScrollHotbar", true);

            hotbarActions = new InputAction[8];
            for (int i = 0; i < hotbarActions.Length; i++)
            {
                hotbarActions[i] = playerMap.FindAction($"Hotbar{i + 1}", false);
            }
        }

        private void OnEnable()
        {
            playerMap?.Enable();
        }

        private void OnDisable()
        {
            playerMap?.Disable();
        }

        public bool WasJumpPressedThisFrame()
        {
            return jumpAction != null && jumpAction.WasPressedThisFrame();
        }

        public bool WasInteractPressedThisFrame()
        {
            return interactAction != null && interactAction.WasPressedThisFrame();
        }

        public float ReadScrollValue()
        {
            return scrollHotbarAction != null ? scrollHotbarAction.ReadValue<float>() : 0f;
        }

        public int ReadHotbarNumberPressed()
        {
            if (hotbarActions == null)
                return -1;

            for (int i = 0; i < hotbarActions.Length; i++)
            {
                if (hotbarActions[i] != null && hotbarActions[i].WasPressedThisFrame())
                    return i;
            }

            return -1;
        }
    }
}