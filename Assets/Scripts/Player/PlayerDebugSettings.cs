using UnityEngine;

namespace Islebound.Player
{
    public class PlayerDebugSettings : MonoBehaviour
    {
        [Header("General")]
        [SerializeField] private bool useLiveTuning = true;
        [SerializeField] private bool applyCurrentStatOverridesOnStart = true;
        [SerializeField] private bool applyCurrentStatOverridesContinuously = false;

        [Header("Movement")]
        [SerializeField] private bool freezeMovement = false;
        [SerializeField] private float walkSpeed = 4f;
        [SerializeField] private float sprintSpeed = 7f;
        [SerializeField] private float jumpHeight = 1.35f;
        [SerializeField] private float gravity = -25f;
        [SerializeField] private float coyoteTime = 0.15f;
        [SerializeField] private float groundCheckRadius = 0.22f;

        [Header("Look")]
        [SerializeField] private bool freezeLook = false;
        [SerializeField] private float mouseSensitivity = 0.12f;
        [SerializeField] private float minPitch = -75f;
        [SerializeField] private float maxPitch = 75f;

        [Header("Interaction")]
        [SerializeField] private float interactDistance = 3f;

        [Header("Max Stats")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float maxHunger = 100f;
        [SerializeField] private float maxWater = 100f;
        [SerializeField] private float maxStamina = 100f;

        [Header("Current Stats Overrides")]
        [SerializeField] private float currentHealth = 100f;
        [SerializeField] private float currentHunger = 100f;
        [SerializeField] private float currentWater = 100f;
        [SerializeField] private float currentStamina = 100f;

        [Header("Survival Drain")]
        [SerializeField] private bool disableSurvivalDrain = false;
        [SerializeField] private float hungerDrainPerSecond = 0.35f;
        [SerializeField] private float waterDrainPerSecond = 0.5f;
        [SerializeField] private float starvationDamagePerSecond = 3f;

        [Header("Stamina")]
        [SerializeField] private bool infiniteStamina = false;
        [SerializeField] private bool alwaysCanSprint = false;
        [SerializeField] private bool freeJump = false;
        [SerializeField] private float staminaRecoveryPerSecond = 18f;
        [SerializeField] private float staminaSprintDrainPerSecond = 22f;
        [SerializeField] private float jumpStaminaCost = 15f;

        [Header("God Mode")]
        [SerializeField] private bool infiniteHealth = false;
        [SerializeField] private bool infiniteHunger = false;
        [SerializeField] private bool infiniteWater = false;

        [Header("Hotkeys")]
        [SerializeField] private bool enableDebugHotkeys = true;
        [SerializeField] private KeyCode refillAllKey = KeyCode.F1;
        [SerializeField] private KeyCode emptyHungerWaterKey = KeyCode.F2;
        [SerializeField] private KeyCode damageHealthKey = KeyCode.F3;
        [SerializeField] private KeyCode toggleInfiniteStaminaKey = KeyCode.F4;
        [SerializeField] private KeyCode toggleNoDrainKey = KeyCode.F5;

        private PlayerStats playerStats;

        public bool UseLiveTuning => useLiveTuning;
        public bool ApplyCurrentStatOverridesOnStart => applyCurrentStatOverridesOnStart;
        public bool ApplyCurrentStatOverridesContinuously => applyCurrentStatOverridesContinuously;

        public bool FreezeMovement => freezeMovement;
        public float WalkSpeed => walkSpeed;
        public float SprintSpeed => sprintSpeed;
        public float JumpHeight => jumpHeight;
        public float Gravity => gravity;
        public float CoyoteTime => coyoteTime;
        public float GroundCheckRadius => groundCheckRadius;

        public bool FreezeLook => freezeLook;
        public float MouseSensitivity => mouseSensitivity;
        public float MinPitch => minPitch;
        public float MaxPitch => maxPitch;

        public float InteractDistance => interactDistance;

        public float MaxHealth => maxHealth;
        public float MaxHunger => maxHunger;
        public float MaxWater => maxWater;
        public float MaxStamina => maxStamina;

        public float CurrentHealthOverride => currentHealth;
        public float CurrentHungerOverride => currentHunger;
        public float CurrentWaterOverride => currentWater;
        public float CurrentStaminaOverride => currentStamina;

        public bool DisableSurvivalDrain => disableSurvivalDrain;
        public float HungerDrainPerSecond => hungerDrainPerSecond;
        public float WaterDrainPerSecond => waterDrainPerSecond;
        public float StarvationDamagePerSecond => starvationDamagePerSecond;

        public bool InfiniteStamina => infiniteStamina;
        public bool AlwaysCanSprint => alwaysCanSprint;
        public bool FreeJump => freeJump;
        public float StaminaRecoveryPerSecond => staminaRecoveryPerSecond;
        public float StaminaSprintDrainPerSecond => staminaSprintDrainPerSecond;
        public float JumpStaminaCost => jumpStaminaCost;

        public bool InfiniteHealth => infiniteHealth;
        public bool InfiniteHunger => infiniteHunger;
        public bool InfiniteWater => infiniteWater;

        private void Awake()
        {
            playerStats = GetComponent<PlayerStats>();
        }

        private void Update()
        {
            if (!enableDebugHotkeys || playerStats == null)
                return;

            if (Input.GetKeyDown(refillAllKey))
            {
                playerStats.RefillAllStats();
                Debug.Log("Debug: Refill all stats");
            }

            if (Input.GetKeyDown(emptyHungerWaterKey))
            {
                playerStats.SetHunger(0f);
                playerStats.SetWater(0f);
                Debug.Log("Debug: Hunger and Water set to 0");
            }

            if (Input.GetKeyDown(damageHealthKey))
            {
                playerStats.SetHealth(playerStats.CurrentHealth - 25f);
                Debug.Log("Debug: Health -25");
            }

            if (Input.GetKeyDown(toggleInfiniteStaminaKey))
            {
                infiniteStamina = !infiniteStamina;
                Debug.Log($"Debug: Infinite Stamina = {infiniteStamina}");
            }

            if (Input.GetKeyDown(toggleNoDrainKey))
            {
                disableSurvivalDrain = !disableSurvivalDrain;
                Debug.Log($"Debug: Disable Survival Drain = {disableSurvivalDrain}");
            }
        }

        private void OnValidate()
        {
            walkSpeed = Mathf.Max(0f, walkSpeed);
            sprintSpeed = Mathf.Max(0f, sprintSpeed);
            jumpHeight = Mathf.Max(0f, jumpHeight);
            coyoteTime = Mathf.Max(0f, coyoteTime);
            groundCheckRadius = Mathf.Max(0.01f, groundCheckRadius);

            mouseSensitivity = Mathf.Max(0f, mouseSensitivity);
            interactDistance = Mathf.Max(0.1f, interactDistance);

            maxHealth = Mathf.Max(1f, maxHealth);
            maxHunger = Mathf.Max(1f, maxHunger);
            maxWater = Mathf.Max(1f, maxWater);
            maxStamina = Mathf.Max(1f, maxStamina);

            currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
            currentHunger = Mathf.Clamp(currentHunger, 0f, maxHunger);
            currentWater = Mathf.Clamp(currentWater, 0f, maxWater);
            currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);

            hungerDrainPerSecond = Mathf.Max(0f, hungerDrainPerSecond);
            waterDrainPerSecond = Mathf.Max(0f, waterDrainPerSecond);
            starvationDamagePerSecond = Mathf.Max(0f, starvationDamagePerSecond);

            staminaRecoveryPerSecond = Mathf.Max(0f, staminaRecoveryPerSecond);
            staminaSprintDrainPerSecond = Mathf.Max(0f, staminaSprintDrainPerSecond);
            jumpStaminaCost = Mathf.Max(0f, jumpStaminaCost);
        }
    }
}