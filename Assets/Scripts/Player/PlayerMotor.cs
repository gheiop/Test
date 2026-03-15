using UnityEngine;

namespace Islebound.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMotor : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform groundCheck;

        [Header("Fallback Movement")]
        [SerializeField] private float walkSpeed = 4f;
        [SerializeField] private float sprintSpeed = 7f;
        [SerializeField] private float jumpHeight = 1.35f;
        [SerializeField] private float gravity = -25f;

        [Header("Fallback Ground Check")]
        [SerializeField] private float groundCheckRadius = 0.22f;
        [SerializeField] private LayerMask groundMask;

        [Header("Fallback Jump Assist")]
        [SerializeField] private float coyoteTime = 0.15f;

        private CharacterController controller;
        private PlayerDebugSettings debugSettings;

        private Vector3 velocity;
        private bool isGrounded;
        private float coyoteTimeCounter;

        public bool IsGrounded => isGrounded;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            debugSettings = GetComponent<PlayerDebugSettings>();
        }

        private void Update()
        {
            UpdateGroundedState();
            UpdateCoyoteTime();
        }

        public void Move(Vector2 input, Transform orientation, bool sprinting)
        {
            UpdateGroundedState();

            if (IsMovementFrozen())
                input = Vector2.zero;

            if (isGrounded && velocity.y < 0f)
            {
                velocity.y = -2f;
            }

            Vector3 forward = orientation.forward;
            Vector3 right = orientation.right;

            forward.y = 0f;
            right.y = 0f;

            forward.Normalize();
            right.Normalize();

            Vector3 moveDirection = (forward * input.y + right * input.x).normalized;
            float currentSpeed = sprinting ? GetSprintSpeed() : GetWalkSpeed();

            Vector3 horizontalVelocity = moveDirection * currentSpeed;

            velocity.y += GetGravity() * Time.deltaTime;

            Vector3 finalMove = horizontalVelocity;
            finalMove.y = velocity.y;

            controller.Move(finalMove * Time.deltaTime);
        }

        public bool TryJump()
        {
            UpdateGroundedState();

            bool canJump = isGrounded || coyoteTimeCounter > 0f;
            if (!canJump)
                return false;

            velocity.y = Mathf.Sqrt(GetJumpHeight() * -2f * GetGravity());
            coyoteTimeCounter = 0f;
            return true;
        }

        private void UpdateGroundedState()
        {
            bool sphereGrounded = Physics.CheckSphere(
                groundCheck.position,
                GetGroundCheckRadius(),
                groundMask,
                QueryTriggerInteraction.Ignore);

            isGrounded = controller.isGrounded || sphereGrounded;
        }

        private void UpdateCoyoteTime()
        {
            if (isGrounded)
                coyoteTimeCounter = GetCoyoteTime();
            else
                coyoteTimeCounter -= Time.deltaTime;
        }

        private bool HasLiveDebugSettings()
        {
            return debugSettings != null && debugSettings.UseLiveTuning;
        }

        private bool IsMovementFrozen()
        {
            return HasLiveDebugSettings() && debugSettings.FreezeMovement;
        }

        private float GetWalkSpeed() => HasLiveDebugSettings() ? debugSettings.WalkSpeed : walkSpeed;
        private float GetSprintSpeed() => HasLiveDebugSettings() ? debugSettings.SprintSpeed : sprintSpeed;
        private float GetJumpHeight() => HasLiveDebugSettings() ? debugSettings.JumpHeight : jumpHeight;
        private float GetGravity() => HasLiveDebugSettings() ? debugSettings.Gravity : gravity;
        private float GetGroundCheckRadius() => HasLiveDebugSettings() ? debugSettings.GroundCheckRadius : groundCheckRadius;
        private float GetCoyoteTime() => HasLiveDebugSettings() ? debugSettings.CoyoteTime : coyoteTime;

        private void OnDrawGizmosSelected()
        {
            if (groundCheck == null)
                return;

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}