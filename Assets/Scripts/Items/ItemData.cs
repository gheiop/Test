using UnityEngine;

namespace Islebound.Items
{
    public enum ItemCategory
    {
        Resource,
        Ore,
        Tool,
        Weapon,
        Artifact,
        Consumable,
        Quest
    }

    public enum ItemRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }

    [CreateAssetMenu(fileName = "SO_Item_", menuName = "Islebound/Items/Item Data")]
    public class ItemData : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string itemId = "item_id";
        [SerializeField] private string displayName = "New Item";
        [TextArea(2, 5)]
        [SerializeField] private string description = "Item description.";

        [Header("Classification")]
        [SerializeField] private ItemCategory category = ItemCategory.Resource;
        [SerializeField] private ItemRarity rarity = ItemRarity.Common;

        [Header("Stack")]
        [SerializeField] private bool stackable = true;
        [SerializeField] private int maxStack = 99;

        [Header("Visual")]
        [SerializeField] private Sprite icon;
        [SerializeField] private Color iconTint = Color.white;
        [SerializeField] private GameObject worldPrefab;

        [Header("Tool Settings")]
        [SerializeField] private bool isTool;
        [SerializeField] private int durability = 100;
        [SerializeField] private float gatherPower = 1f;

        [Header("Debug")]
        [SerializeField] private bool debugLogWhenUsed;

        public string ItemId => itemId;
        public string DisplayName => displayName;
        public string Description => description;
        public ItemCategory Category => category;
        public ItemRarity Rarity => rarity;
        public bool Stackable => stackable;
        public int MaxStack => Mathf.Max(1, maxStack);
        public Sprite Icon => icon;
        public Color IconTint => iconTint;
        public GameObject WorldPrefab => worldPrefab;
        public bool IsTool => isTool;
        public int Durability => durability;
        public float GatherPower => gatherPower;
        public bool DebugLogWhenUsed => debugLogWhenUsed;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                itemId = name.ToLower().Replace(" ", "_");
            }

            if (maxStack < 1)
            {
                maxStack = 1;
            }

            if (!stackable)
            {
                maxStack = 1;
            }

            if (durability < 1)
            {
                durability = 1;
            }

            if (gatherPower < 0f)
            {
                gatherPower = 0f;
            }
        }
#endif
    }
}