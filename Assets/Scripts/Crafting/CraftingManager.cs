using System.Text;
using UnityEngine;
using Islebound.Items;

namespace Islebound.Crafting
{
    public class CraftingManager : MonoBehaviour
    {
        public static CraftingManager Instance { get; private set; }

        [Header("Debug")]
        [SerializeField] private bool debugLogs = true;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public bool CanCraft(CraftingRecipeData recipe, int craftCount = 1)
        {
            if (recipe == null || craftCount <= 0)
                return false;

            if (InventoryManager.Instance == null)
                return false;

            CraftingIngredient[] ingredients = recipe.Ingredients;
            if (ingredients == null || ingredients.Length == 0)
                return recipe.OutputItem != null;

            for (int i = 0; i < ingredients.Length; i++)
            {
                CraftingIngredient ingredient = ingredients[i];
                if (ingredient == null || ingredient.Item == null)
                    return false;

                int requiredAmount = ingredient.Amount * craftCount;
                int currentAmount = InventoryManager.Instance.GetTotalAmount(ingredient.Item);

                if (currentAmount < requiredAmount)
                    return false;
            }

            return recipe.OutputItem != null;
        }

        public bool Craft(CraftingRecipeData recipe, int craftCount = 1)
        {
            if (recipe == null || craftCount <= 0)
                return false;

            if (InventoryManager.Instance == null)
            {
                if (debugLogs)
                {
                    Debug.LogWarning("[CraftingManager] InventoryManager.Instance is missing.");
                }

                return false;
            }

            if (!CanCraft(recipe, craftCount))
            {
                if (debugLogs)
                {
                    Debug.LogWarning($"[CraftingManager] Cannot craft recipe: {recipe.DisplayName}");
                }

                return false;
            }

            CraftingIngredient[] ingredients = recipe.Ingredients;

            for (int i = 0; i < ingredients.Length; i++)
            {
                CraftingIngredient ingredient = ingredients[i];
                InventoryManager.Instance.RemoveItem(ingredient.Item, ingredient.Amount * craftCount);
            }

            bool added = InventoryManager.Instance.AddItem(recipe.OutputItem, recipe.OutputAmount * craftCount);

            if (!added)
            {
                if (debugLogs)
                {
                    Debug.LogWarning($"[CraftingManager] Crafted item could not be fully added. Refunding ingredients for {recipe.DisplayName}");
                }

                for (int i = 0; i < ingredients.Length; i++)
                {
                    CraftingIngredient ingredient = ingredients[i];
                    InventoryManager.Instance.AddItem(ingredient.Item, ingredient.Amount * craftCount);
                }

                return false;
            }

            if (debugLogs)
            {
                Debug.Log($"[CraftingManager] Crafted {recipe.OutputAmount * craftCount} x {recipe.OutputItem.DisplayName}");
            }

            return true;
        }

        public string BuildIngredientsText(CraftingRecipeData recipe)
        {
            if (recipe == null)
                return "No recipe selected.";

            if (InventoryManager.Instance == null)
                return "InventoryManager missing.";

            StringBuilder builder = new StringBuilder();
            CraftingIngredient[] ingredients = recipe.Ingredients;

            if (ingredients == null || ingredients.Length == 0)
            {
                builder.Append("No ingredients.");
                return builder.ToString();
            }

            for (int i = 0; i < ingredients.Length; i++)
            {
                CraftingIngredient ingredient = ingredients[i];

                if (ingredient == null || ingredient.Item == null)
                    continue;

                int currentAmount = InventoryManager.Instance.GetTotalAmount(ingredient.Item);
                builder.Append("- ");
                builder.Append(ingredient.Item.DisplayName);
                builder.Append(" x");
                builder.Append(ingredient.Amount);
                builder.Append(" (have ");
                builder.Append(currentAmount);
                builder.Append(")");

                if (i < ingredients.Length - 1)
                {
                    builder.AppendLine();
                }
            }

            return builder.ToString();
        }
    }
}