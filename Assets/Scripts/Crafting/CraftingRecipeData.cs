using UnityEngine;
using Islebound.Items;

namespace Islebound.Crafting
{
    [System.Serializable]
    public class CraftingIngredient
    {
        [SerializeField] private ItemData item;
        [SerializeField] private int amount = 1;

        public ItemData Item => item;
        public int Amount => Mathf.Max(1, amount);
    }

    [CreateAssetMenu(fileName = "SO_Recipe_", menuName = "Islebound/Crafting/Recipe Data")]
    public class CraftingRecipeData : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string recipeId = "recipe_id";
        [SerializeField] private string displayName = "New Recipe";
        [TextArea(2, 5)]
        [SerializeField] private string description = "Recipe description.";

        [Header("Output")]
        [SerializeField] private ItemData outputItem;
        [SerializeField] private int outputAmount = 1;
        [SerializeField] private Sprite iconOverride;

        [Header("Requirements")]
        [SerializeField] private CraftingIngredient[] ingredients;
        [SerializeField] private bool unlockedByDefault = true;

        [Header("Debug")]
        [SerializeField] private bool debugLogs;

        public string RecipeId => recipeId;
        public string DisplayName => displayName;
        public string Description => description;
        public ItemData OutputItem => outputItem;
        public int OutputAmount => Mathf.Max(1, outputAmount);
        public Sprite Icon => iconOverride != null ? iconOverride : (outputItem != null ? outputItem.Icon : null);
        public CraftingIngredient[] Ingredients => ingredients;
        public bool UnlockedByDefault => unlockedByDefault;
        public bool DebugLogs => debugLogs;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(recipeId))
            {
                recipeId = name.ToLower().Replace(" ", "_");
            }

            if (outputAmount < 1)
            {
                outputAmount = 1;
            }
        }
#endif
    }
}