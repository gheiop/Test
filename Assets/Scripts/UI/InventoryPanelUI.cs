using UnityEngine;
using Islebound.Core;
using Islebound.Items;

namespace Islebound.UI
{
    public class InventoryPanelUI : MonoBehaviour
    {
        [SerializeField] private InventorySlotUI[] slots;
        [SerializeField] private bool debugLogs = false;

        private InventorySlot[] cachedSlots;

        private void OnEnable()
        {
            GameEvents.OnInventoryChanged += RefreshInventory;

            if (InventoryManager.Instance != null)
            {
                cachedSlots = InventoryManager.Instance.InventorySlots;
                Redraw();

                if (debugLogs)
                {
                    Debug.Log("[InventoryPanelUI] OnEnable -> refreshed from InventoryManager.");
                }
            }
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

            if (debugLogs)
            {
                Debug.Log("[InventoryPanelUI] RefreshInventory event received.");
            }
        }

        private void Redraw()
        {
            if (slots == null || cachedSlots == null)
                return;

            int count = Mathf.Min(slots.Length, cachedSlots.Length);

            for (int i = 0; i < count; i++)
            {
                if (slots[i] != null)
                {
                    slots[i].SetData(cachedSlots[i]);
                }
            }
        }
    }
}