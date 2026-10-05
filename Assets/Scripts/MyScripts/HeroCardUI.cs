using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum CardMode { Shop, Collection }

public class HeroCardUI : MonoBehaviour
{
    [Header("Режим работы")]
    [SerializeField] private CardMode mode = CardMode.Shop;

    [Header("Данные героя")]
    [SerializeField] private HeroConfig heroConfig;
    [SerializeField] private bool isDefaultHero = false;

    [Header("UI элементы")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text priceOrStatusText;
    [SerializeField] private Button actionButton;
    [SerializeField] private TMP_Text buttonText;

    private ShopPanelUI shopPanel;
    private HeroCollectionUI collectionPanel;

    private void Start()
    {
        shopPanel = GetComponentInParent<ShopPanelUI>();
        collectionPanel = GetComponentInParent<HeroCollectionUI>();

        if (shopPanel != null) shopPanel.OnShopUpdated += RefreshCard;
        if (collectionPanel != null) collectionPanel.OnCollectionUpdated += RefreshCard;

        if (actionButton != null) actionButton.onClick.AddListener(OnCardClicked);

        RefreshCard();
    }

    private void OnDestroy()
    {
        if (shopPanel != null) shopPanel.OnShopUpdated -= RefreshCard;
        if (collectionPanel != null) collectionPanel.OnCollectionUpdated -= RefreshCard;
    }

    public void RefreshCard()
    {
        if (heroConfig == null) return;
        if (nameText != null) nameText.text = heroConfig.heroName;

        bool isUnlocked = IsHeroUnlocked();
        bool isSelected = IsHeroSelected();

        if (mode == CardMode.Shop)
        {
            if (isUnlocked)
            {
                if (priceOrStatusText != null) priceOrStatusText.text = "РАЗБЛОКИРОВАН";
                if (buttonText != null) buttonText.text = "КУПЛЕНО";
                if (actionButton != null) actionButton.interactable = false;
            }
            else
            {
                if (priceOrStatusText != null) priceOrStatusText.text = $"{heroConfig.price} МОНЕТ";
                if (buttonText != null) buttonText.text = "КУПИТЬ";
                if (actionButton != null) actionButton.interactable = true;
            }
        }
        else if (mode == CardMode.Collection)
        {
            if (!isUnlocked)
            {
                if (priceOrStatusText != null) priceOrStatusText.text = "ЗАКРЫТО";
                if (buttonText != null) buttonText.text = "КУПИТЕ В МАГАЗИНЕ";
                if (actionButton != null) actionButton.interactable = false;
            }
            else if (isSelected)
            {
                if (priceOrStatusText != null) priceOrStatusText.text = "ВЫБРАН";
                if (buttonText != null) buttonText.text = "АКТИВЕН";
                if (actionButton != null) actionButton.interactable = false;
            }
            else
            {
                if (priceOrStatusText != null) priceOrStatusText.text = "ДОСТУПЕН";
                if (buttonText != null) buttonText.text = "ВЫБРАТЬ";
                if (actionButton != null) actionButton.interactable = true;
            }
        }
    }

    private void OnCardClicked()
    {
        if (heroConfig == null) return;

        if (mode == CardMode.Shop && shopPanel != null)
        {
            shopPanel.TryBuyHero(heroConfig, isDefaultHero);
        }
        else if (mode == CardMode.Collection && collectionPanel != null)
        {
            collectionPanel.SelectHero(heroConfig.heroId);
        }
    }

    private bool IsHeroUnlocked()
    {
        if (isDefaultHero) return true;
        if (GameDataManager.Instance == null || GameDataManager.Instance.Profile == null) return false;

        // Проверяем, есть ли ID героя в списке профиля из JSON/Google Drive
        return GameDataManager.Instance.Profile.unlockedCharacterIds.Contains(heroConfig.heroId);
    }

    private bool IsHeroSelected()
    {
        if (GameDataManager.Instance == null || GameDataManager.Instance.Profile == null) return false;
        return GameDataManager.Instance.Profile.selectedCharacterId == heroConfig.heroId;
    }
}