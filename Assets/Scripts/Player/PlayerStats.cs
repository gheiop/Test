using UnityEngine;
using Islebound.Core;

namespace Islebound.Player
{
    public class PlayerStats : MonoBehaviour
    {
        [Header("Fallback Maximum Values")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float maxHunger = 100f;
        [SerializeField] private float maxWater = 100f;
        [SerializeField] private float maxStamina = 100f;

        [Header("Current Values")]
        [SerializeField] private float currentHealth = 100f;
        [SerializeField] private float currentHunger = 100f;
        [SerializeField] private float currentWater = 100f;
        [SerializeField] private float currentStamina = 100f;

        [Header("Fallback Survival Drain")]
        [SerializeField] private float hungerDrainPerSecond = 0.35f;
        [SerializeField] private float waterDrainPerSecond = 0.5f;
        [SerializeField] private float starvationDamagePerSecond = 3f;

        [Header("Fallback Stamina")]
        [SerializeField] private float staminaRecoveryPerSecond = 18f;
        [SerializeField] private float staminaSprintDrainPerSecond = 22f;
        [SerializeField] private float jumpStaminaCost = 15f;

        private PlayerDebugSettings debugSettings;

        public float CurrentHealth => currentHealth;
        public float CurrentHunger => currentHunger;
        public float CurrentWater => currentWater;
        public float CurrentStamina => currentStamina;
        public float JumpStaminaCost => GetJumpStaminaCost();

        private void Awake()
        {
            debugSettings = GetComponent<PlayerDebugSettings>();
        }

        private void Start()
        {
            if (HasLiveDebugSettings() && debugSettings.ApplyCurrentStatOverridesOnStart)
            {
                ApplyCurrentOverridesFromDebug();
            }

            BroadcastAll();
        }

        private void Update()
        {
            if (HasLiveDebugSettings() && debugSettings.ApplyCurrentStatOverridesContinuously)
            {
                ApplyCurrentOverridesFromDebug();
            }

            ApplyDebugCheats();
            DrainSurvivalNeeds(Time.deltaTime);
        }

        public void DrainSurvivalNeeds(float deltaTime)
        {
            if (HasLiveDebugSettings() && debugSettings.DisableSurvivalDrain)
                return;

            if (!IsInfiniteHunger())
                SetHunger(currentHunger - GetHungerDrainPerSecond() * deltaTime);

            if (!IsInfiniteWater())
                SetWater(currentWater - GetWaterDrainPerSecond() * deltaTime);

            if ((currentHunger <= 0f || currentWater <= 0f) && !IsInfiniteHealth())
            {
                SetHealth(currentHealth - GetStarvationDamagePerSecond() * deltaTime);
            }
        }

        public void RecoverStamina(float deltaTime)
        {
            if (IsInfiniteStamina())
            {
                SetStamina(GetMaxStamina());
                return;
            }

            SetStamina(currentStamina + GetStaminaRecoveryPerSecond() * deltaTime);
        }

        public void DrainSprintStamina(float deltaTime)
        {
            if (IsInfiniteStamina())
            {
                SetStamina(GetMaxStamina());
                return;
            }

            SetStamina(currentStamina - GetStaminaSprintDrainPerSecond() * deltaTime);
        }

        public bool CanSprint()
        {
            if (HasLiveDebugSettings() && debugSettings.AlwaysCanSprint)
                return true;

            return currentStamina > 1f;
        }

        public bool TryConsumeJumpStamina()
        {
            if (HasLiveDebugSettings() && debugSettings.FreeJump)
                return true;

            if (IsInfiniteStamina())
                return true;

            float cost = GetJumpStaminaCost();
            if (currentStamina < cost)
                return false;

            SetStamina(currentStamina - cost);
            return true;
        }

        public void RefillAllStats()
        {
            SetHealth(GetMaxHealth());
            SetHunger(GetMaxHunger());
            SetWater(GetMaxWater());
            SetStamina(GetMaxStamina());
        }

        public void SetHealth(float value)
        {
            currentHealth = Mathf.Clamp(value, 0f, GetMaxHealth());
            GameEvents.OnHealthChanged?.Invoke(currentHealth, GetMaxHealth());
        }

        public void SetHunger(float value)
        {
            currentHunger = Mathf.Clamp(value, 0f, GetMaxHunger());
            GameEvents.OnHungerChanged?.Invoke(currentHunger, GetMaxHunger());
        }

        public void SetWater(float value)
        {
            currentWater = Mathf.Clamp(value, 0f, GetMaxWater());
            GameEvents.OnWaterChanged?.Invoke(currentWater, GetMaxWater());
        }

        public void SetStamina(float value)
        {
            currentStamina = Mathf.Clamp(value, 0f, GetMaxStamina());
            GameEvents.OnStaminaChanged?.Invoke(currentStamina, GetMaxStamina());
        }

        private void ApplyCurrentOverridesFromDebug()
        {
            SetHealth(debugSettings.CurrentHealthOverride);
            SetHunger(debugSettings.CurrentHungerOverride);
            SetWater(debugSettings.CurrentWaterOverride);
            SetStamina(debugSettings.CurrentStaminaOverride);
        }

        private void ApplyDebugCheats()
        {
            if (IsInfiniteHealth() && currentHealth < GetMaxHealth())
                SetHealth(GetMaxHealth());

            if (IsInfiniteHunger() && currentHunger < GetMaxHunger())
                SetHunger(GetMaxHunger());

            if (IsInfiniteWater() && currentWater < GetMaxWater())
                SetWater(GetMaxWater());

            if (IsInfiniteStamina() && currentStamina < GetMaxStamina())
                SetStamina(GetMaxStamina());
        }

        private void BroadcastAll()
        {
            GameEvents.OnHealthChanged?.Invoke(currentHealth, GetMaxHealth());
            GameEvents.OnHungerChanged?.Invoke(currentHunger, GetMaxHunger());
            GameEvents.OnWaterChanged?.Invoke(currentWater, GetMaxWater());
            GameEvents.OnStaminaChanged?.Invoke(currentStamina, GetMaxStamina());
        }

        private bool HasLiveDebugSettings()
        {
            return debugSettings != null && debugSettings.UseLiveTuning;
        }

        private bool IsInfiniteHealth() => HasLiveDebugSettings() && debugSettings.InfiniteHealth;
        private bool IsInfiniteHunger() => HasLiveDebugSettings() && debugSettings.InfiniteHunger;
        private bool IsInfiniteWater() => HasLiveDebugSettings() && debugSettings.InfiniteWater;
        private bool IsInfiniteStamina() => HasLiveDebugSettings() && debugSettings.InfiniteStamina;

        private float GetMaxHealth() => HasLiveDebugSettings() ? debugSettings.MaxHealth : maxHealth;
        private float GetMaxHunger() => HasLiveDebugSettings() ? debugSettings.MaxHunger : maxHunger;
        private float GetMaxWater() => HasLiveDebugSettings() ? debugSettings.MaxWater : maxWater;
        private float GetMaxStamina() => HasLiveDebugSettings() ? debugSettings.MaxStamina : maxStamina;

        private float GetHungerDrainPerSecond() => HasLiveDebugSettings() ? debugSettings.HungerDrainPerSecond : hungerDrainPerSecond;
        private float GetWaterDrainPerSecond() => HasLiveDebugSettings() ? debugSettings.WaterDrainPerSecond : waterDrainPerSecond;
        private float GetStarvationDamagePerSecond() => HasLiveDebugSettings() ? debugSettings.StarvationDamagePerSecond : starvationDamagePerSecond;

        private float GetStaminaRecoveryPerSecond() => HasLiveDebugSettings() ? debugSettings.StaminaRecoveryPerSecond : staminaRecoveryPerSecond;
        private float GetStaminaSprintDrainPerSecond() => HasLiveDebugSettings() ? debugSettings.StaminaSprintDrainPerSecond : staminaSprintDrainPerSecond;
        private float GetJumpStaminaCost() => HasLiveDebugSettings() ? debugSettings.JumpStaminaCost : jumpStaminaCost;
    }
}