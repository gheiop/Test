using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Islebound.Core;

namespace Islebound.UI
{
    public class HUDController : MonoBehaviour
    {
        [Header("Health")]
        [SerializeField] private TMP_Text healthValueText;

        [Header("Bars")]
        [SerializeField] private Image hungerFill;
        [SerializeField] private Image waterFill;
        [SerializeField] private Image staminaFill;

        [Header("Optional Value Labels")]
        [SerializeField] private TMP_Text hungerValueText;
        [SerializeField] private TMP_Text waterValueText;
        [SerializeField] private TMP_Text staminaValueText;

        [Header("Interaction UI")]
        [SerializeField] private TMP_Text interactionPromptText;
        [SerializeField] private GameObject interactionPromptRoot;

        private void OnEnable()
        {
            GameEvents.OnHealthChanged += UpdateHealth;
            GameEvents.OnHungerChanged += UpdateHunger;
            GameEvents.OnWaterChanged += UpdateWater;
            GameEvents.OnStaminaChanged += UpdateStamina;
            GameEvents.OnInteractionPromptShown += ShowInteractionPrompt;
            GameEvents.OnInteractionPromptHidden += HideInteractionPrompt;
        }

        private void OnDisable()
        {
            GameEvents.OnHealthChanged -= UpdateHealth;
            GameEvents.OnHungerChanged -= UpdateHunger;
            GameEvents.OnWaterChanged -= UpdateWater;
            GameEvents.OnStaminaChanged -= UpdateStamina;
            GameEvents.OnInteractionPromptShown -= ShowInteractionPrompt;
            GameEvents.OnInteractionPromptHidden -= HideInteractionPrompt;
        }

        private void Start()
        {
            HideInteractionPrompt();
        }

        private void UpdateHealth(float current, float max)
        {
            if (healthValueText != null)
                healthValueText.text = $"{Mathf.CeilToInt(current)}";
        }

        private void UpdateHunger(float current, float max)
        {
            if (hungerFill != null)
                hungerFill.fillAmount = max > 0f ? current / max : 0f;

            if (hungerValueText != null)
                hungerValueText.text = $"{Mathf.CeilToInt(current)}/{Mathf.CeilToInt(max)}";
        }

        private void UpdateWater(float current, float max)
        {
            if (waterFill != null)
                waterFill.fillAmount = max > 0f ? current / max : 0f;

            if (waterValueText != null)
                waterValueText.text = $"{Mathf.CeilToInt(current)}/{Mathf.CeilToInt(max)}";
        }

        private void UpdateStamina(float current, float max)
        {
            if (staminaFill != null)
                staminaFill.fillAmount = max > 0f ? current / max : 0f;

            if (staminaValueText != null)
                staminaValueText.text = $"{Mathf.CeilToInt(current)}/{Mathf.CeilToInt(max)}";
        }

        private void ShowInteractionPrompt(string text)
        {
            if (interactionPromptRoot != null)
                interactionPromptRoot.SetActive(true);

            if (interactionPromptText != null)
                interactionPromptText.text = text;
        }

        private void HideInteractionPrompt()
        {
            if (interactionPromptRoot != null)
                interactionPromptRoot.SetActive(false);
        }
    }
}