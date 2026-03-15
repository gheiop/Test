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
    }

    public class InventoryManager : MonoBehaviour
    {
        public static InventoryManager Instance { get; private set; }

        [Header("Hotbar")]
        [SerializeField] private int hotbarSize = 8;
        [SerializeField] private InventorySlot[] hotbarSlots;
        [SerializeField] private int selectedHotbarIndex;

        public int HotbarSize => hotbarSize;
        public int SelectedHotbarIndex => selectedHotbarIndex;
        public InventorySlot[] HotbarSlots => hotbarSlots;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            hotbarSlots = new InventorySlot[hotbarSize];
            for (int i = 0; i < hotbarSlots.Length; i++)
            {
                hotbarSlots[i] = new InventorySlot();
            }
        }

        private void Start()
        {
            NotifyInventoryChanged();
            NotifyHotbarSelectionChanged();
        }

        public bool AddItem(ItemData itemData, int amount = 1)
        {
            if (itemData == null || amount <= 0)
                return false;

            if (itemData.Stackable)
            {
                for (int i = 0; i < hotbarSlots.Length; i++)
                {
                    if (!hotbarSlots[i].IsEmpty && hotbarSlots[i].Item.Id == itemData.Id)
                    {
                        hotbarSlots[i].Amount += amount;
                        NotifyInventoryChanged();
                        return true;
                    }
                }
            }

            for (int i = 0; i < hotbarSlots.Length; i++)
            {
                if (hotbarSlots[i].IsEmpty)
                {
                    hotbarSlots[i].Item = itemData;
                    hotbarSlots[i].Amount = amount;
                    NotifyInventoryChanged();
                    return true;
                }
            }

            Debug.Log("Hotbar is full.");
            return false;
        }

        public void SelectHotbarIndex(int index)
        {
            if (index < 0 || index >= hotbarSlots.Length)
                return;

            selectedHotbarIndex = index;
            NotifyHotbarSelectionChanged();
        }

        public void ScrollSelection(int direction)
        {
            if (hotbarSlots.Length == 0)
                return;

            selectedHotbarIndex -= direction;

            if (selectedHotbarIndex < 0)
                selectedHotbarIndex = hotbarSlots.Length - 1;
            else if (selectedHotbarIndex >= hotbarSlots.Length)
                selectedHotbarIndex = 0;

            NotifyHotbarSelectionChanged();
        }

        private void NotifyInventoryChanged()
        {
            GameEvents.OnInventoryChanged?.Invoke(hotbarSlots);
        }

        private void NotifyHotbarSelectionChanged()
        {
            GameEvents.OnHotbarSelectionChanged?.Invoke(selectedHotbarIndex);
        }
    }
}