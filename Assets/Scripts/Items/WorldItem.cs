using UnityEngine;

namespace Islebound.Items
{
    [RequireComponent(typeof(Collider))]
    public class WorldItem : MonoBehaviour, IInteractable
    {
        [SerializeField] private ItemData itemData;
        [SerializeField] private int amount = 1;
        [SerializeField] private bool destroyOnPickup = true;

        public string GetInteractionText()
        {
            if (itemData == null)
                return "Pick Up";

            return $"[E] Pick up {itemData.DisplayName}";
        }

        public void Interact()
        {
            if (itemData == null)
                return;

            bool added = InventoryManager.Instance != null &&
                         InventoryManager.Instance.AddItem(itemData, amount);

            if (!added)
                return;

            if (destroyOnPickup)
            {
                Destroy(gameObject);
            }
        }
    }
}