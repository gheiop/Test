using UnityEngine;
using Islebound.Core;

namespace Islebound.Items
{
    [System.Serializable]
    public class InventorySlot
    {
        public ItemData Item;
        public int Amount;

        public bool IsEmpty => Item == null || Amount <= 0;

        public void Clear()
        {
            Item = null;
            Amount = 0;
        }

        public void Set(ItemData item, int amount)
        {
            Item = item;
            Amount = amount;
        }

        public bool CanStack(ItemData item)
        {
            if (item == null || Item == null)
                return false;

            return Item == item && Item.Stackable && Amount < Item.MaxStack;
        }

        public int AddToStack(int amountToAdd)
        {
            if (Item == null || !Item.Stackable || amountToAdd <= 0)
                return amountToAdd;

            int freeSpace = Item.MaxStack - Amount;
            int added = Mathf.Min(freeSpace, amountToAdd);
            Amount += added;
            return amountToAdd - added;
        }

        public int RemoveAmount(int amountToRemove)
        {
            if (IsEmpty || amountToRemove <= 0)
                return amountToRemove;

            int removed = Mathf.Min(Amount, amountToRemove);
            Amount -= removed;

            if (Amount <= 0)
            {
                Clear();
            }

            return amountToRemove - removed;
        }
    }

    public class InventoryManager : MonoBehaviour
    {
        public static InventoryManager Instance { get; private set; }

        [Header("Size")]
        [SerializeField] private int hotbarSize = 8;
        [SerializeField] private int inventorySize = 24;

        [Header("Behavior")]
        [SerializeField] private bool addToHotbarFirst = true;
        [SerializeField] private bool inventoryOpen;
        [SerializeField] private int selectedHotbarIndex;

        [Header("Debug")]
        [SerializeField] private bool debugLogs = true;
        [SerializeField] private bool debugAddTestItemsOnStart;
        [SerializeField] private ItemData[] debugStartItems;

        [Header("Runtime")]
        [SerializeField] private InventorySlot[] hotbarSlots;
        [SerializeField] private InventorySlot[] inventorySlots;

        public int HotbarSize => hotbarSize;
        public int InventorySize => inventorySize;
        public int SelectedHotbarIndex => selectedHotbarIndex;
        public bool IsInventoryOpen => inventoryOpen;
        public InventorySlot[] HotbarSlots => hotbarSlots;
        public InventorySlot[] InventorySlots => inventorySlots;

        public InventorySlot SelectedHotbarSlot
        {
            get
            {
                if (hotbarSlots == null || hotbarSlots.Length == 0)
                    return null;

                if (selectedHotbarIndex < 0 || selectedHotbarIndex >= hotbarSlots.Length)
                    return null;

                return hotbarSlots[selectedHotbarIndex];
            }
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            hotbarSize = Mathf.Max(1, hotbarSize);
            inventorySize = Mathf.Max(1, inventorySize);

            hotbarSlots = CreateSlotsArray(hotbarSize);
            inventorySlots = CreateSlotsArray(inventorySize);

            selectedHotbarIndex = Mathf.Clamp(selectedHotbarIndex, 0, hotbarSlots.Length - 1);
        }

        private void Start()
        {
            if (debugAddTestItemsOnStart && debugStartItems != null)
            {
                for (int i = 0; i < debugStartItems.Length; i++)
                {
                    if (debugStartItems[i] != null)
                    {
                        AddItem(debugStartItems[i], 1);
                    }
                }
            }

            BroadcastAll();
        }

        private InventorySlot[] CreateSlotsArray(int size)
        {
            InventorySlot[] slots = new InventorySlot[size];

            for (int i = 0; i < size; i++)
            {
                slots[i] = new InventorySlot();
            }

            return slots;
        }

        public bool AddItem(ItemData itemData, int amount = 1)
        {
            if (itemData == null || amount <= 0)
                return false;

            int remaining = amount;

            if (addToHotbarFirst)
            {
                remaining = AddItemToSlots(hotbarSlots, itemData, remaining);
                remaining = AddItemToSlots(inventorySlots, itemData, remaining);
            }
            else
            {
                remaining = AddItemToSlots(inventorySlots, itemData, remaining);
                remaining = AddItemToSlots(hotbarSlots, itemData, remaining);
            }

            int addedAmount = amount - remaining;

            if (addedAmount > 0)
            {
                if (debugLogs)
                {
                    Debug.Log($"[InventoryManager] Added {addedAmount} x {itemData.DisplayName}");
                }

                GameEvents.OnItemPickedUp?.Invoke(itemData, addedAmount);
                BroadcastAll();
            }

            if (remaining > 0 && debugLogs)
            {
                Debug.LogWarning($"[InventoryManager] Inventory full. Could not add {remaining} x {itemData.DisplayName}");
            }

            return remaining == 0;
        }

        private int AddItemToSlots(InventorySlot[] slots, ItemData itemData, int amount)
        {
            if (slots == null || itemData == null || amount <= 0)
                return amount;

            if (itemData.Stackable)
            {
                for (int i = 0; i < slots.Length; i++)
                {
                    if (slots[i].CanStack(itemData))
                    {
                        amount = slots[i].AddToStack(amount);
                        if (amount <= 0)
                            return 0;
                    }
                }
            }

            for (int i = 0; i < slots.Length; i++)
            {
                if (!slots[i].IsEmpty)
                    continue;

                int placeAmount = itemData.Stackable
                    ? Mathf.Min(amount, itemData.MaxStack)
                    : 1;

                slots[i].Set(itemData, placeAmount);
                amount -= placeAmount;

                if (amount <= 0)
                    return 0;
            }

            return amount;
        }

        public bool HasItem(ItemData itemData, int requiredAmount)
        {
            if (itemData == null || requiredAmount <= 0)
                return false;

            return GetTotalAmount(itemData) >= requiredAmount;
        }

        public int GetTotalAmount(ItemData itemData)
        {
            if (itemData == null)
                return 0;

            int total = 0;
            total += CountInSlots(hotbarSlots, itemData);
            total += CountInSlots(inventorySlots, itemData);
            return total;
        }

        private int CountInSlots(InventorySlot[] slots, ItemData itemData)
        {
            if (slots == null || itemData == null)
                return 0;

            int total = 0;

            for (int i = 0; i < slots.Length; i++)
            {
                if (!slots[i].IsEmpty && slots[i].Item == itemData)
                {
                    total += slots[i].Amount;
                }
            }

            return total;
        }

        public bool RemoveItem(ItemData itemData, int amount)
        {
            if (itemData == null || amount <= 0)
                return false;

            if (!HasItem(itemData, amount))
                return false;

            int remaining = amount;

            remaining = RemoveItemFromSlots(inventorySlots, itemData, remaining);
            remaining = RemoveItemFromSlots(hotbarSlots, itemData, remaining);

            BroadcastAll();
            return remaining == 0;
        }

        private int RemoveItemFromSlots(InventorySlot[] slots, ItemData itemData, int amount)
        {
            if (slots == null || itemData == null || amount <= 0)
                return amount;

            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i].IsEmpty || slots[i].Item != itemData)
                    continue;

                amount = slots[i].RemoveAmount(amount);

                if (amount <= 0)
                    return 0;
            }

            return amount;
        }

        public void SelectHotbarIndex(int index)
        {
            if (hotbarSlots == null || hotbarSlots.Length == 0)
                return;

            if (index < 0 || index >= hotbarSlots.Length)
                return;

            selectedHotbarIndex = index;
            GameEvents.OnHotbarSelectionChanged?.Invoke(selectedHotbarIndex);
        }

        public void ScrollSelection(int direction)
        {
            if (hotbarSlots == null || hotbarSlots.Length == 0)
                return;

            selectedHotbarIndex -= direction;

            if (selectedHotbarIndex < 0)
            {
                selectedHotbarIndex = hotbarSlots.Length - 1;
            }
            else if (selectedHotbarIndex >= hotbarSlots.Length)
            {
                selectedHotbarIndex = 0;
            }

            GameEvents.OnHotbarSelectionChanged?.Invoke(selectedHotbarIndex);
        }

        public bool MoveInventorySlotToHotbar(int inventoryIndex, int hotbarIndex)
        {
            if (!IsValidIndex(inventorySlots, inventoryIndex) || !IsValidIndex(hotbarSlots, hotbarIndex))
                return false;

            SwapSlots(inventorySlots[inventoryIndex], hotbarSlots[hotbarIndex]);
            BroadcastAll();
            return true;
        }

        public bool SwapHotbarSlots(int firstIndex, int secondIndex)
        {
            if (!IsValidIndex(hotbarSlots, firstIndex) || !IsValidIndex(hotbarSlots, secondIndex))
                return false;

            SwapSlots(hotbarSlots[firstIndex], hotbarSlots[secondIndex]);
            BroadcastAll();
            return true;
        }

        public bool SwapInventorySlots(int firstIndex, int secondIndex)
        {
            if (!IsValidIndex(inventorySlots, firstIndex) || !IsValidIndex(inventorySlots, secondIndex))
                return false;

            SwapSlots(inventorySlots[firstIndex], inventorySlots[secondIndex]);
            BroadcastAll();
            return true;
        }

        private bool IsValidIndex(InventorySlot[] slots, int index)
        {
            return slots != null && index >= 0 && index < slots.Length;
        }

        private void SwapSlots(InventorySlot a, InventorySlot b)
        {
            ItemData tempItem = a.Item;
            int tempAmount = a.Amount;

            a.Item = b.Item;
            a.Amount = b.Amount;

            b.Item = tempItem;
            b.Amount = tempAmount;
        }

        public void ToggleInventoryScreen()
        {
            SetInventoryScreenState(!inventoryOpen);
        }

        public void SetInventoryScreenState(bool isOpen)
        {
            inventoryOpen = isOpen;
            GameEvents.OnInventoryScreenStateChanged?.Invoke(inventoryOpen);

            if (debugLogs)
            {
                Debug.Log($"[InventoryManager] Inventory screen: {(inventoryOpen ? "OPEN" : "CLOSED")}");
            }
        }

        public void ForceRefreshUI()
        {
            BroadcastAll();
        }

        private void BroadcastAll()
        {
            GameEvents.OnHotbarChanged?.Invoke(hotbarSlots);
            GameEvents.OnInventoryChanged?.Invoke(inventorySlots);
            GameEvents.OnHotbarSelectionChanged?.Invoke(selectedHotbarIndex);
            GameEvents.OnInventoryScreenStateChanged?.Invoke(inventoryOpen);
        }

        [ContextMenu("Debug Print Inventory Contents")]
private void DebugPrintInventoryContents()
{
    Debug.Log("=== HOTBAR ===");
    for (int i = 0; i < hotbarSlots.Length; i++)
    {
        string itemName = hotbarSlots[i].IsEmpty ? "EMPTY" : hotbarSlots[i].Item.DisplayName;
        int amount = hotbarSlots[i].IsEmpty ? 0 : hotbarSlots[i].Amount;
        Debug.Log($"Hotbar[{i}] = {itemName} x{amount}");
    }

    Debug.Log("=== INVENTORY ===");
    for (int i = 0; i < inventorySlots.Length; i++)
    {
        string itemName = inventorySlots[i].IsEmpty ? "EMPTY" : inventorySlots[i].Item.DisplayName;
        int amount = inventorySlots[i].IsEmpty ? 0 : inventorySlots[i].Amount;
        Debug.Log($"Inventory[{i}] = {itemName} x{amount}");
    }
}
    }
}