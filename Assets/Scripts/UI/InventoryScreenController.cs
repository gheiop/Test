using UnityEngine;
using Islebound.Core;
using Islebound.Items;

namespace Islebound.UI
{
    public class InventoryScreenController : MonoBehaviour
    {
        [SerializeField] private GameObject inventoryRoot;
        [SerializeField] private KeyCode toggleKey = KeyCode.Tab;
        [SerializeField] private bool closeWithEscape = true;

        private void OnEnable()
        {
            GameEvents.OnInventoryScreenStateChanged += ApplyState;
        }

        private void OnDisable()
        {
            GameEvents.OnInventoryScreenStateChanged -= ApplyState;
        }

        private void Start()
        {
            bool initialState = InventoryManager.Instance != null && InventoryManager.Instance.IsInventoryOpen;
            ApplyState(initialState);
        }

        private void Update()
        {
            if (InventoryManager.Instance == null)
                return;

            if (Input.GetKeyDown(toggleKey))
            {
                InventoryManager.Instance.ToggleInventoryScreen();
            }

            if (closeWithEscape && Input.GetKeyDown(KeyCode.Escape) && InventoryManager.Instance.IsInventoryOpen)
            {
                InventoryManager.Instance.SetInventoryScreenState(false);
            }
        }

        private void ApplyState(bool isOpen)
        {
            if (inventoryRoot != null)
            {
                inventoryRoot.SetActive(isOpen);
            }
        }
    }
}