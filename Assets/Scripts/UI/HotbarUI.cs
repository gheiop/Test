using UnityEngine;
using Islebound.Core;
using Islebound.Items;

namespace Islebound.UI
{
    public class HotbarUI : MonoBehaviour
    {
        [SerializeField] private HotbarSlotUI[] slots;
        [SerializeField] private bool debugLogs = false;

        private InventorySlot[] cachedSlots;
        private int selectedIndex;

        private void OnEnable()
        {
            GameEvents.OnHotbarChanged += RefreshHotbar;
            GameEvents.OnHotbarSelectionChanged += RefreshSelection;

            if (InventoryManager.Instance != null)
            {
                cachedSlots = InventoryManager.Instance.HotbarSlots;
                selectedIndex = InventoryManager.Instance.SelectedHotbarIndex;
                Redraw();

                if (debugLogs)
                {
                    Debug.Log("[HotbarUI] OnEnable -> refreshed from InventoryManager.");
                }
            }
        }

        private void OnDisable()
        {
            GameEvents.OnHotbarChanged -= RefreshHotbar;
            GameEvents.OnHotbarSelectionChanged -= RefreshSelection;
        }

        private void Start()
        {
            if (InventoryManager.Instance == null)
                return;

            cachedSlots = InventoryManager.Instance.HotbarSlots;
            selectedIndex = InventoryManager.Instance.SelectedHotbarIndex;
            Redraw();
        }

        private void RefreshHotbar(InventorySlot[] hotbarSlots)
        {
            cachedSlots = hotbarSlots;
            Redraw();
        }

        private void RefreshSelection(int index)
        {
            selectedIndex = index;
            Redraw();
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
                    slots[i].SetData(cachedSlots[i], i == selectedIndex);
                }
            }
        }
    }
}