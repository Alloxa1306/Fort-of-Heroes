using UnityEngine;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Button slotButton;

    private ItemConfig itemData;
    private PlayerInventory playerInventory;

    public void SetupSlot(ItemConfig item, PlayerInventory inventory)
    {
        itemData = item;
        playerInventory = inventory;

        if (item != null && item.icon != null)
        {
            iconImage.sprite = item.icon;
            iconImage.enabled = true;
        }
        else
        {
            iconImage.enabled = false;
        }

        slotButton.onClick.RemoveAllListeners();
        slotButton.onClick.AddListener(OnSlotClicked);
    }

    private void OnSlotClicked()
    {
        if (itemData != null && playerInventory != null)
        {
            playerInventory.UseItem(itemData);
        }
    }
}