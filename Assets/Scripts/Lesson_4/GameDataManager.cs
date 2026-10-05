using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameDataManager : MonoBehaviour
{
    public static GameDataManager Instance { get; private set; }

    // Конфигурации
    public PlayerConfig PlayerConfig { get; private set; }
    public EnemyConfig EnemyConfig { get; private set; }

    // Профиль игрока
    public PlayerStats Profile { get; private set; } = new PlayerStats();

    // Системные настройки
    public GameSettings Settings { get; private set; } = new GameSettings();

    public MapInfo SelectedMapInfo { get; set; }

    private string profileSavePath;
    private string settingsSavePath;
    private string cloudFileName; // Уникальное имя файла для Google Диска

    public bool isDataLoaded { get; private set; } = false;

    private void Awake()
    {
        Application.targetFrameRate = 200;
        QualitySettings.vSyncCount = 0;

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Получаем или создаем уникальный ID устройства
        string devicePlayerId = PlayerPrefs.GetString("DevicePlayerID", "");

        // Если запускаем игру впервые — генерируем новый ID
        if (string.IsNullOrEmpty(devicePlayerId))
        {
            devicePlayerId = System.Guid.NewGuid().ToString().Substring(0, 8);
            PlayerPrefs.SetString("DevicePlayerID", devicePlayerId);
            PlayerPrefs.Save();
        }

        string finalId = devicePlayerId;

        
        // Проверяем, запускается ли игра в редакторе и есть ли в пути слово "clone"
#if UNITY_EDITOR
        if (Application.dataPath.Contains("clone"))
        {
            finalId += "_clone";
        }
#endif

        // Формируем уникальные пути
        cloudFileName = $"GameData_{finalId}.json";
        profileSavePath = Path.Combine(Application.persistentDataPath, cloudFileName);

        // Настройки графики и звука
        settingsSavePath = Path.Combine(Application.persistentDataPath, "Settings.json");
    }

    private async void Start()
    {
        // Запускаем единый процесс загрузки всего на свете
        await LoadAllDataAsync();

        // Запускаем локальный автосейв профиля раз в минуту
        InvokeRepeating(nameof(AutoSaveProfileLocal), 60f, 60f);
    }

    private async Task LoadAllDataAsync()
    {
        // ПРОВЕРКА: Если это планшет/телефон (Android или iOS)
        if (Application.isMobilePlatform)
        {
            Debug.Log("[GameDataManager] Мобильная платформа. Загрузка только локальных данных.");

            // Грузим стандартные конфиги для автономной игры
            PlayerConfig = new PlayerConfig();
            EnemyConfig = new EnemyConfig();

            LoadSettingsLocal();

            if (File.Exists(profileSavePath))
            {
                LoadProfileFromLocalFile();
            }
            else
            {
                Profile = new PlayerStats();
                isDataLoaded = true;
            }

            return; 
        }

        // Загрузка конфигов
        string playerJson = await GoogleDriveTools.DownloadFileAsync("PlayerConfig.json");
        PlayerConfig = !string.IsNullOrEmpty(playerJson)
            ? JsonUtility.FromJson<PlayerConfig>(playerJson)
            : new PlayerConfig();

        string enemyJson = await GoogleDriveTools.DownloadFileAsync("EnemyConfig.json");
        EnemyConfig = !string.IsNullOrEmpty(enemyJson)
            ? JsonUtility.FromJson<EnemyConfig>(enemyJson)
            : new EnemyConfig();

        //  Загрузка настроек
        LoadSettingsLocal();

        //  Загрузка персонального профиля
        // Используем уникальное имя файла
        string dataJson = await GoogleDriveTools.DownloadFileAsync(cloudFileName);

        if (!string.IsNullOrEmpty(dataJson))
        {
            File.WriteAllText(profileSavePath, dataJson);
            LoadProfileFromLocalFile();
        }
        else
        {
            // Если в облаке нет файла, проверяем локальный сейв
            if (File.Exists(profileSavePath))
            {
                LoadProfileFromLocalFile();
            }
            else
            {
                Profile = new PlayerStats();
                isDataLoaded = true;
            }
        }

        Debug.Log("[GameDataManager] Все модули данных успешно инициализированы!");
    }

    // Метод для профиля
    public async Task SavePlayerProfile(bool syncWithCloud)
    {
        if (Profile == null) return;

        string jsonString = JsonUtility.ToJson(Profile);
        File.WriteAllText(profileSavePath, jsonString);
        Debug.Log("[SaveSystem] Профиль сохранен локально.");

        // Отправляем в облако ТОЛЬКО если просят и это НЕ планшет
        if (syncWithCloud && !Application.isMobilePlatform)
        {
            await GoogleDriveTools.UploadAsync(cloudFileName, jsonString);
            Debug.Log($"[SaveSystem] Профиль {cloudFileName} синхронизирован с Google Drive!");
        }
    }

    private void LoadProfileFromLocalFile()
    {
        if (File.Exists(profileSavePath))
        {
            string json = File.ReadAllText(profileSavePath);
            Profile = JsonUtility.FromJson<PlayerStats>(json);
            isDataLoaded = true;
        }
    }

    private async void AutoSaveProfileLocal()
    {
        if (isDataLoaded)
        {
            await SavePlayerProfile(false);
        }
    }

    // Метод для настроек(Settings)
    public void SaveSettingsLocal()
    {
        string json = JsonUtility.ToJson(Settings);
        File.WriteAllText(settingsSavePath, json);
        Debug.Log("[SaveSystem] Системные настройки сохранены локально.");
    }

    private void LoadSettingsLocal()
    {
        if (File.Exists(settingsSavePath))
        {
            string json = File.ReadAllText(settingsSavePath);
            Settings = JsonUtility.FromJson<GameSettings>(json);
        }
        else
        {
            Settings = new GameSettings(); 
        }
    }

    private async void OnApplicationQuit()
    {
        if (isDataLoaded)
        {
            // При выходе жестко сохраняем всё и в облако
            await SavePlayerProfile(true);
            SaveSettingsLocal();
        }
    }

    public string GetSavePath() => profileSavePath;
}