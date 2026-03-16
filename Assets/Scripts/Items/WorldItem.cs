using UnityEngine;
using Islebound.Items;

namespace Islebound.Items
{
    [RequireComponent(typeof(Collider))]
    public class WorldItem : MonoBehaviour, IInteractable
    {
        [Header("Item")]
        [SerializeField] private ItemData itemData;
        [SerializeField] private int amount = 1;

        [Header("Behavior")]
        [SerializeField] private bool destroyOnPickup = true;
        [SerializeField] private bool debugLogs = false;

        public string GetInteractionText()
        {
            if (itemData == null)
                return "[E] Pick Up";

            if (amount > 1)
                return $"[E] Pick up {itemData.DisplayName} x{amount}";

            return $"[E] Pick up {itemData.DisplayName}";
        }

        public void Interact()
        {
            if (itemData == null)
            {
                if (debugLogs)
                {
                    Debug.LogWarning("[WorldItem] Missing ItemData.");
                }
                return;
            }

            if (InventoryManager.Instance == null)
            {
                if (debugLogs)
                {
                    Debug.LogWarning("[WorldItem] InventoryManager.Instance is missing.");
                }
                return;
            }

            bool added = InventoryManager.Instance.AddItem(itemData, Mathf.Max(1, amount));

            if (!added)
            {
                if (debugLogs)
                {
                    Debug.Log("[WorldItem] Item was not fully picked up because inventory is full.");
                }
                return;
            }

            if (destroyOnPickup)
            {
                Destroy(gameObject);
            }
        }
    }
}