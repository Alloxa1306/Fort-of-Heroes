using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Окна")]
    [SerializeField] private GameObject mainMenuWindow;
    [SerializeField] private GameObject mapSelectionWindow;

    [Header("Имя сцены Gameplay (fallback)")]
    [SerializeField] private string gameplaySceneName = "GamePlayScene";

    [Header("Обновление данных в UI")]
    [SerializeField] private TextMeshProUGUI shopCoinsText;
    [SerializeField] private TextMeshProUGUI statsKillsText;

    [Header("Ссылка на NetworkLauncher")]
    [SerializeField] private RelayLauncher relayLauncher;

    public GameObject MainMenuWindow => mainMenuWindow;

    private Stack<GameObject> windowHistory = new Stack<GameObject>();

    private GameObject currentWindow;

    private void Start()
    {
        if (mainMenuWindow != null)
        {
            OpenWindowInternal(mainMenuWindow, false);
        }

        UpdateProfileUI();
    }

    private void OnEnable()
    {
        UpdateProfileUI();
    }

    public void UpdateProfileUI()
    {
        if (GameDataManager.Instance != null && GameDataManager.Instance.Profile != null)
        {
            if (shopCoinsText != null)
            {
                shopCoinsText.text = GameDataManager.Instance.Profile.coinsCount.ToString();
            }

            if (statsKillsText != null)
            {
                statsKillsText.text = $"Убийств врагов: {GameDataManager.Instance.Profile.killsCount}";
            }
        }
    }

    public void OpenWindow(GameObject nextWindow)
    {
        OpenWindowInternal(nextWindow, true);
        UpdateProfileUI();
    }

    public void GoBack()
    {
        if (windowHistory.Count == 0)
        {
            return;
        }

        if (currentWindow != null)
        {
            currentWindow.SetActive(false);
        }

        currentWindow = windowHistory.Pop();
        currentWindow.SetActive(true);

        UpdateProfileUI();
    }

    private void OpenWindowInternal(GameObject nextWindow, bool saveToHistory)
    {
        if (nextWindow == null) return;

        if (currentWindow != null)
        {
            if (saveToHistory)
            {
                windowHistory.Push(currentWindow);
            }
            currentWindow.SetActive(false);
        }

        currentWindow = nextWindow;
        currentWindow.SetActive(true);
    }

    public void PlayGame()
    {
        if (mapSelectionWindow != null)
        {
            OpenWindow(mapSelectionWindow);
        }
        else
        {
            Debug.LogError("[UIManager] Панель выбора карт не назначена!");
            if (relayLauncher != null)
            {
                relayLauncher.CreateRelayGame();
            }
            else
            {
                SceneManager.LoadScene(gameplaySceneName);
            }
        }
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
