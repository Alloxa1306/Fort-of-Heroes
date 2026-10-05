using System;
using UnityEngine;

public class RewardReceiver : MonoBehaviour
{
    public void Give100Coins()
    {
        if (GameDataManager.Instance != null && GameDataManager.Instance.Profile != null)
        {
            GameDataManager.Instance.Profile.coinsCount += 100;
            _ = GameDataManager.Instance.SavePlayerProfile(true);

            var shop = FindObjectOfType<ShopPanelUI>();
            if (shop != null) shop.UpdateCoinsUI();

            var uiManager = FindObjectOfType<UIManager>();
            if (uiManager != null) uiManager.UpdateProfileUI();

            Debug.Log("Монеты выданы через IAP Button!");
        }
    }
}