using UnityEngine;
using Islebound.Core;
using Islebound.Items;

namespace Islebound.UI
{
    public class InventoryPanelUI : MonoBehaviour
    {
        [SerializeField] private InventorySlotUI[] slots;

        private InventorySlot[] cachedSlots;

        private void OnEnable()
        {
            GameEvents.OnInventoryChanged += RefreshInventory;
        }

        private void OnDisable()
        {
            GameEvents.OnInventoryChanged -= RefreshInventory;
        }

        private void Start()
        {
            if (InventoryManager.Instance == null)
                return;

            cachedSlots = InventoryManager.Instance.InventorySlots;
            Redraw();
        }

        private void RefreshInventory(InventorySlot[] inventorySlots)
        {
            cachedSlots = inventorySlots;
            Redraw();
        }

        private void Redraw()
        {
            if (slots == null || cachedSlots == null)
                return;

            int count = Mathf.Min(slots.Length, cachedSlots.Length);

            for (int i = 0; i < count; i++)
            {
                slots[i].SetData(cachedSlots[i]);
            }
        }
    }
}