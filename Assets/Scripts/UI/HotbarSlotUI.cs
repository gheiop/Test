using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Islebound.Items;

namespace Islebound.UI
{
    public class HotbarSlotUI : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TMP_Text amountText;
        [SerializeField] private GameObject selectionHighlight;
        [SerializeField] private RectTransform slotRoot;

        [Header("Scale")]
        [SerializeField] private Vector3 normalScale = Vector3.one;
        [SerializeField] private Vector3 selectedScale = new Vector3(1.15f, 1.15f, 1.15f);

        public void SetData(InventorySlot slot, bool selected)
        {
            bool hasItem = slot != null && !slot.IsEmpty;

            if (iconImage != null)
            {
                iconImage.enabled = hasItem;
                iconImage.sprite = hasItem ? slot.Item.Icon : null;
                iconImage.color = hasItem ? Color.white : new Color(1f, 1f, 1f, 0f);
            }

            if (amountText != null)
            {
                bool showAmount = hasItem && slot.Amount > 1;
                amountText.text = showAmount ? slot.Amount.ToString() : string.Empty;
                amountText.enabled = showAmount;
            }

            if (selectionHighlight != null)
                selectionHighlight.SetActive(selected);

            if (slotRoot != null)
                slotRoot.localScale = selected ? selectedScale : normalScale;
        }
    }
}