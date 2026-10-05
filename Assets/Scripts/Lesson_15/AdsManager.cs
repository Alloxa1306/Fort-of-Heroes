using UnityEngine;
using UnityEngine.Advertisements;

public class AdsManager : MonoBehaviour, IUnityAdsInitializationListener, IUnityAdsLoadListener, IUnityAdsShowListener
{
    public static AdsManager Instance { get; private set; }

    [Header("Настройки Unity Ads")]
    [SerializeField] private string androidGameId = "1234567"; // ID из дашборда
    [SerializeField] private string iosGameId = "1234568";
    [SerializeField] private bool testMode = true;

    private string rewardedAdUnitId;
    private string gameId;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Определяем платформу
        gameId = (Application.platform == RuntimePlatform.IPhonePlayer) ? iosGameId : androidGameId;
        rewardedAdUnitId = (Application.platform == RuntimePlatform.IPhonePlayer) ? "Rewarded_iOS" : "Rewarded_Android";

        // Инициализация
        Advertisement.Initialize(gameId, testMode, this);
    }

    // Метод для вызова с кнопки в UI
    public void ShowRewardedAd()
    {
        Debug.Log("[Ads] Загрузка ролика...");
        Advertisement.Load(rewardedAdUnitId, this);
    }

    // Интерфейс инициализации
    public void OnInitializationComplete() => Debug.Log("[Ads] Инициализация успешна.");
    public void OnInitializationFailed(UnityAdsInitializationError error, string message) => Debug.LogError($"[Ads] Ошибка инициализации: {error} - {message}");

    // Интерфейс загрузки
    public void OnUnityAdsAdLoaded(string adUnitId)
    {
        Debug.Log("[Ads] Ролик загружен, показываем...");
        Advertisement.Show(adUnitId, this);
    }
    public void OnUnityAdsFailedToLoad(string adUnitId, UnityAdsLoadError error, string message) => Debug.LogError($"[Ads] Ошибка загрузки ролика: {error} - {message}");

    // Интерфейс показа и НАГРАДЫ 
    public void OnUnityAdsShowComplete(string adUnitId, UnityAdsShowCompletionState showCompletionState)
    {
        if (adUnitId == rewardedAdUnitId && showCompletionState == UnityAdsShowCompletionState.COMPLETED)
        {
            Debug.Log("[Ads] Ролик просмотрен до конца! Выдаем награду.");
            GiveRewardToPlayer(50); // Награда 50 монет
        }
    }

    public void OnUnityAdsShowFailure(string adUnitId, UnityAdsShowError error, string message) => Debug.LogError($"[Ads] Ошибка показа: {error} - {message}");
    public void OnUnityAdsShowStart(string adUnitId) { }
    public void OnUnityAdsShowClick(string adUnitId) { }

    private void GiveRewardToPlayer(int coinsAmount)
    {
        if (GameDataManager.Instance != null && GameDataManager.Instance.Profile != null)
        {
            GameDataManager.Instance.Profile.coinsCount += coinsAmount;

            // Сохраняем профиль
            _ = GameDataManager.Instance.SavePlayerProfile(true);

            // Обновляем UI, если мы находимся в магазине
            var shop = FindObjectOfType<ShopPanelUI>();
            if (shop != null) shop.UpdateCoinsUI();

            var uiManager = FindObjectOfType<UIManager>();
            if (uiManager != null) uiManager.UpdateProfileUI();
        }
    }
}