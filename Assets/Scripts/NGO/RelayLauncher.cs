using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using TMPro;

public class RelayLauncher : MonoBehaviour
{
    [Header("UI Элементы")]
    public TMP_InputField joinInput;
    public TextMeshProUGUI codeDisplayText;
    [SerializeField] private string gamePlayScene;
    public string GamePlayScene { get => gamePlayScene; set => gamePlayScene = value; }

    private async void Start()
    {
        // Блокируем инициализацию облака Unity для телефонов
        if (Application.isMobilePlatform)
        {
            Debug.Log("[Network] Планшет: облачные сервисы Unity отключены.");
            return;
        }

        InitializationOptions options = new InitializationOptions();

#if UNITY_EDITOR
        if (Application.dataPath.Contains("clone"))
        {
            options.SetProfile("Clone");
        }
#endif

        await UnityServices.InitializeAsync(options);

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log($"Авторизован в Unity Cloud. Player ID: {AuthenticationService.Instance.PlayerId}");
        }
    }

    public async void CreateRelayGame()
    {
        // Перехватываем создание комнаты и запускаем локальный хост
        if (Application.isMobilePlatform)
        {
            Debug.Log("[Network] Планшет: Запуск оффлайн режима (Localhost).");
            StartLocalHost();
            return;
        }

        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(3);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            Debug.Log($"Сервер создан! Код: {joinCode}");
            if (codeDisplayText != null) codeDisplayText.text = $"Код комнаты: {joinCode}";

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData
            );

            NetworkManager.Singleton.StartHost();
            NetworkManager.Singleton.SceneManager.LoadScene(gamePlayScene, UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
        catch (RelayServiceException e)
        {
            Debug.LogError(e);
        }
    }

    public async void JoinRelayGame()
    {
        // Блокируем попытку присоединиться к Relay с телефона
        if (Application.isMobilePlatform) return;

        if (joinInput == null || string.IsNullOrEmpty(joinInput.text)) return;

        try
        {
            string joinCode = joinInput.text.Substring(0, 6);
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(
                joinAllocation.RelayServer.IpV4,
                (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.Key,
                joinAllocation.ConnectionData,
                joinAllocation.HostConnectionData
            );

            NetworkManager.Singleton.StartClient();
        }
        catch (RelayServiceException e)
        {
            Debug.LogError(e);
        }
    }

    public void StartLocalHost()
    {
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetConnectionData("127.0.0.1", 7777);

        Debug.Log("Запуск локального сервера (Localhost)");
        NetworkManager.Singleton.StartHost();

        // Используем динамическую переменную gamePlayScene вместо жесткого текста
        NetworkManager.Singleton.SceneManager.LoadScene(gamePlayScene, UnityEngine.SceneManagement.LoadSceneMode.Single);
    }

    public void StartLocalClient()
    {
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetConnectionData("127.0.0.1", 7777);

        Debug.Log("Подключение к локальному серверу...");
        NetworkManager.Singleton.StartClient();
    }
}