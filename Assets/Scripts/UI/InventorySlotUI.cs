using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Islebound.Items;

namespace Islebound.UI
{
    public class InventorySlotUI : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TMP_Text amountText;
        [SerializeField] private GameObject emptyStateObject;

        public void SetData(InventorySlot slot)
        {
            bool hasItem = slot != null && !slot.IsEmpty;

            if (iconImage != null)
            {
                iconImage.enabled = hasItem;
                iconImage.sprite = hasItem ? slot.Item.Icon : null;
                iconImage.color = hasItem ? slot.Item.IconTint : new Color(1f, 1f, 1f, 0f);
            }

            if (amountText != null)
            {
                bool showAmount = hasItem && slot.Amount > 1;
                amountText.text = showAmount ? slot.Amount.ToString() : string.Empty;
                amountText.enabled = showAmount;
            }

            if (emptyStateObject != null)
            {
                emptyStateObject.SetActive(!hasItem);
            }
        }
    }
}