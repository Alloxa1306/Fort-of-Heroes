using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

public class IAPManager : MonoBehaviour, IDetailedStoreListener
{
    public static IAPManager Instance { get; private set; }

    private IStoreController storeController;
    private IExtensionProvider extensionProvider;

    // ID товара
    private const string COINS_100_ID = "coins100";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (storeController == null) InitializePurchasing();
    }

    private void InitializePurchasing()
    {
        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

        // Регистрируем товар как Consumable
        builder.AddProduct(COINS_100_ID, ProductType.Consumable);

        UnityPurchasing.Initialize(this, builder);
    }

    // Вызываем этот метод с кнопки покупки
    public void Buy100Coins()
    {
        if (storeController != null && storeController.products.WithID(COINS_100_ID) != null)
        {
            storeController.InitiatePurchase(COINS_100_ID);
        }
        else
        {
            Debug.LogError("[IAP] Ошибка: магазин не инициализирован или товар не найден.");
        }
    }

    // Обработка успешной покупки
    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        if (string.Equals(args.purchasedProduct.definition.id, COINS_100_ID, System.StringComparison.Ordinal))
        {
            Debug.Log("[IAP] Покупка успешна! Выдаем 100 монет.");
            GiveRewardToPlayer(100);
        }
        else
        {
            Debug.LogWarning($"[IAP] Неизвестный товар: {args.purchasedProduct.definition.id}");
        }
        return PurchaseProcessingResult.Complete;
    }

    private void GiveRewardToPlayer(int coinsAmount)
    {
        if (GameDataManager.Instance != null && GameDataManager.Instance.Profile != null)
        {
            GameDataManager.Instance.Profile.coinsCount += coinsAmount;
            _ = GameDataManager.Instance.SavePlayerProfile(true);

            var shop = FindObjectOfType<ShopPanelUI>();
            if (shop != null) shop.UpdateCoinsUI();

            var uiManager = FindObjectOfType<UIManager>();
            if (uiManager != null) uiManager.UpdateProfileUI();
        }
    }

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        storeController = controller;
        extensionProvider = extensions;
        Debug.Log("[IAP] Инициализация магазина успешна.");
    }
    public void OnInitializeFailed(InitializationFailureReason error) => Debug.LogError($"[IAP] Ошибка инициализации: {error}");
    public void OnInitializeFailed(InitializationFailureReason error, string message) => Debug.LogError($"[IAP] Ошибка инициализации: {error} - {message}");
    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason) => Debug.LogError($"[IAP] Покупка отменена или не удалась: {failureReason}");
    public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription) => Debug.LogError($"[IAP] Ошибка покупки: {failureDescription.message}");
}