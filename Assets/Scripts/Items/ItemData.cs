using UnityEngine;

namespace Islebound.Items
{
    public enum ItemType
    {
        Resource,
        Tool,
        Weapon,
        Artifact,
        Consumable,
        Quest
    }

    [System.Serializable]
    public class ItemData
    {
        public string Id;
        public string DisplayName;
        public ItemType ItemType;
        public Sprite Icon;
        public bool Stackable = true;
        public int MaxStack = 99;
    }
}