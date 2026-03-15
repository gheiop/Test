using UnityEngine;
using Islebound.Core;
using Islebound.Items;

namespace Islebound.UI
{
    public class HotbarUI : MonoBehaviour
    {
        [SerializeField] private HotbarSlotUI[] slots;

        private InventorySlot[] cachedSlots;
        private int currentSelectedIndex;

        private void OnEnable()
        {
            GameEvents.OnInventoryChanged += RefreshInventory;
            GameEvents.OnHotbarSelectionChanged += RefreshSelection;
        }

        private void OnDisable()
        {
            GameEvents.OnInventoryChanged -= RefreshInventory;
            GameEvents.OnHotbarSelectionChanged -= RefreshSelection;
        }

        private void Start()
        {
            if (InventoryManager.Instance != null)
            {
                cachedSlots = InventoryManager.Instance.HotbarSlots;
                currentSelectedIndex = InventoryManager.Instance.SelectedHotbarIndex;
                Redraw();
            }
        }

        private void RefreshInventory(InventorySlot[] inventorySlots)
        {
            cachedSlots = inventorySlots;
            Redraw();
        }

        private void RefreshSelection(int selectedIndex)
        {
            currentSelectedIndex = selectedIndex;
            Redraw();
        }

        private void Redraw()
        {
            if (slots == null || cachedSlots == null)
                return;

            int count = Mathf.Min(slots.Length, cachedSlots.Length);
            for (int i = 0; i < count; i++)
            {
                slots[i].SetData(cachedSlots[i], i == currentSelectedIndex);
            }
        }
    }
}