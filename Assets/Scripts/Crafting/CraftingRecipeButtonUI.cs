using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Islebound.Crafting
{
    public class CraftingRecipeButtonUI : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Image iconImage;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text outputAmountText;
        [SerializeField] private TMP_Text availabilityText;
        [SerializeField] private GameObject selectedHighlight;

        private CraftingRecipeData recipe;
        private Action<CraftingRecipeData> clickCallback;

        public CraftingRecipeData Recipe => recipe;

        private void Awake()
        {
            if (button != null)
            {
                button.onClick.AddListener(HandleClicked);
            }
        }

        public void Setup(CraftingRecipeData recipeData, Action<CraftingRecipeData> onClicked)
        {
            recipe = recipeData;
            clickCallback = onClicked;

            if (iconImage != null)
            {
                iconImage.enabled = recipe != null && recipe.Icon != null;
                iconImage.sprite = recipe != null ? recipe.Icon : null;
            }

            if (nameText != null)
            {
                nameText.text = recipe != null ? recipe.DisplayName : "Missing Recipe";
            }

            if (outputAmountText != null)
            {
                bool showAmount = recipe != null && recipe.OutputAmount > 1;
                outputAmountText.text = showAmount ? $"x{recipe.OutputAmount}" : string.Empty;
            }
        }

        public void RefreshVisual(bool canCraft, bool selected)
        {
            if (availabilityText != null)
            {
                availabilityText.text = canCraft ? "Ready" : "Missing";
            }

            if (selectedHighlight != null)
            {
                selectedHighlight.SetActive(selected);
            }
        }

        private void HandleClicked()
        {
            clickCallback?.Invoke(recipe);
        }
    }
}