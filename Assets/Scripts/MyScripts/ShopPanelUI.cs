using System;
using UnityEngine;
using TMPro;

public class ShopPanelUI : MonoBehaviour
{
    [Header("UI Счётчик монет")]
    [SerializeField] private TMP_Text coinsText;

    [Header("Навигация")]
    [SerializeField] private GameObject mainPanel;

    public event Action OnShopUpdated;

    private void OnEnable()
    {
        UpdateCoinsUI();
        OnShopUpdated?.Invoke();
    }

    public void UpdateCoinsUI()
    {
        if (GameDataManager.Instance != null && GameDataManager.Instance.Profile != null)
        {
            int currentCoins = GameDataManager.Instance.Profile.coinsCount;
            if (coinsText != null) coinsText.text = $"Монеты: {currentCoins}";
        }
    }

    public void TryBuyHero(HeroConfig config, bool isDefault)
    {
        if (config == null) return;
        string heroId = config.heroId;

        if (GameDataManager.Instance == null || GameDataManager.Instance.Profile == null) return;

        var profile = GameDataManager.Instance.Profile;

        if (isDefault || profile.unlockedCharacterIds.Contains(heroId)) return;

        if (profile.coinsCount >= config.price)
        {
            profile.coinsCount -= config.price;
            profile.unlockedCharacterIds.Add(heroId);
            profile.selectedCharacterId = heroId;

            // Мгновенно обновляем интерфейс магазина и счётчик монет
            UpdateCoinsUI();
            OnShopUpdated?.Invoke();

            Debug.Log($"[Shop] Куплен герой: {config.heroName}");

            // Сохраняем в фоновом режиме без блокировки UI
            _ = SaveInBackground();
        }
        else
        {
            Debug.LogWarning("[Shop] Недостаточно монет!");
        }
    }

    private async System.Threading.Tasks.Task SaveInBackground()
    {
        if (GameDataManager.Instance != null)
        {
            await GameDataManager.Instance.SavePlayerProfile(true);
        }
    }

    public void OnClickBack()
    {
        UIManager mainMenuUI = FindObjectOfType<UIManager>();
        if (mainMenuUI != null)
        {
            mainMenuUI.GoBack();
            return;
        }
        GameplayUIManager gameplayUI = FindObjectOfType<GameplayUIManager>();
        if (gameplayUI != null)
        {
            gameplayUI.GoBack();
            return;
        }

        gameObject.SetActive(false); // Если менеджер не найден
    }
}