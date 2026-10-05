using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameplayUIManager : MonoBehaviour
{
    [Header("Окна интерфейса")]
    [SerializeField] private GameObject hudPanel;          // Основной (Static) HUD
    [SerializeField] private GameObject dynamicHudPanel;   // Дополнительный (Dynamic) HUD
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject gameOverPanel;

    [Header("Компоненты для Финала")]
    [SerializeField] private TextMeshProUGUI finalStatsText;
    [SerializeField] private float deathScreenDelay = 2.5f;

    [Header("Имя сцены Главного Меню")]
    [SerializeField] private string mainMenuSceneName = "StartScene";

    private Stack<GameObject> windowHistory = new Stack<GameObject>();
    private GameObject currentWindow;
    private PlayerStatsManager statsManager;
    private GameplayHUD gameplayHUD;

    private bool isPaused = false;

    private void Start()
    {
        Time.timeScale = 1f;
        statsManager = Object.FindAnyObjectByType<PlayerStatsManager>();
        gameplayHUD = Object.FindAnyObjectByType<GameplayHUD>();

        if (hudPanel != null)
        {
            OpenWindowInternal(hudPanel, false);
        }

        if (gameplayHUD != null)
        {
            gameplayHUD.InitializeHUD();
            gameplayHUD.StartTimer();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HandleEscapeKey();
        }
    }

    private void HandleEscapeKey()
    {
        if (currentWindow == hudPanel)
        {
            SetPauseState(true);
            OpenWindow(pausePanel);
        }
        else if (currentWindow == pausePanel)
        {
            ResumeGame();
        }
        else
        {
            GoBack();
        }
    }

    private void SetPauseState(bool state)
    {
        isPaused = state;

        if (isPaused)
        {
            // В меню паузы: курсор полностью свободен и видим
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Confined;

            // Оставляем курсор видимым, чтобы ты мог целиться
            Cursor.visible = true;
        }
    }

    public void ResumeGame()
    {
        SetPauseState(false);
        windowHistory.Clear();

        if (currentWindow != null)
        {
            currentWindow.SetActive(false);
        }

        currentWindow = hudPanel;

        if (hudPanel != null) hudPanel.SetActive(true);
        // Синхронно включаем второй канвас при возврате в игру
        if (dynamicHudPanel != null) dynamicHudPanel.SetActive(true);
    }

    public void OpenWindow(GameObject nextWindow) => OpenWindowInternal(nextWindow, true);

    public void GoBack()
    {
        if (windowHistory.Count == 0) return;

        if (currentWindow != null)
        {
            currentWindow.SetActive(false);
            // Если уходим из HUD назад, выключаем и динамический канвас
            if (currentWindow == hudPanel && dynamicHudPanel != null) dynamicHudPanel.SetActive(false);
        }

        currentWindow = windowHistory.Pop();
        currentWindow.SetActive(true);

        // Если вернулись в HUD, включаем динамический канвас обратно
        if (currentWindow == hudPanel && dynamicHudPanel != null) dynamicHudPanel.SetActive(true);

        if (currentWindow == hudPanel)
        {
            SetPauseState(false);
        }
    }

    private void OpenWindowInternal(GameObject nextWindow, bool saveToHistory)
    {
        if (nextWindow == null) return;

        if (currentWindow != null)
        {
            if (saveToHistory) windowHistory.Push(currentWindow);
            currentWindow.SetActive(false);

            // Прячем второй канвас, если уходим с игрового экрана (например, в паузу)
            if (currentWindow == hudPanel && dynamicHudPanel != null)
            {
                dynamicHudPanel.SetActive(false);
            }
        }

        currentWindow = nextWindow;
        currentWindow.SetActive(true);

        // Показываем второй канвас, если открыли игровой экран
        if (currentWindow == hudPanel && dynamicHudPanel != null)
        {
            dynamicHudPanel.SetActive(true);
        }
    }

    // ИЗМЕНЕНИЯ ЗДЕСЬ
    public void TriggerGameOver()
    {
        // 1. Таймер останавливаем сразу, чтобы зафиксировать точное время выживания
        if (gameplayHUD != null) gameplayHUD.StopTimer();

        // 2. Скрываем игровые панели (полоски ХП, инвентарь), чтобы сделать экран чище перед смертью
        if (hudPanel != null) hudPanel.SetActive(false);
        if (dynamicHudPanel != null) dynamicHudPanel.SetActive(false);

        // 3. Запускаем таймер до появления финального окна
        StartCoroutine(ShowDeathScreenWithDelay());
    }

    // НОВАЯ КОРУТИНА
    private IEnumerator ShowDeathScreenWithDelay()
    {
        // Ждем, пока доиграет анимация падения
        yield return new WaitForSeconds(deathScreenDelay);

        // Открываем панель с видео и показываем курсор
        SetPauseState(true);
        OpenWindow(gameOverPanel);
        ShowFinalStats();
    }

    private void ShowFinalStats()
    {
        if (statsManager == null || finalStatsText == null) return;

        int shots = statsManager.stats.shotsCount;
        int hits = statsManager.stats.hitsCount;
        float accuracy = shots > 0 ? ((float)hits / shots) * 100f : 0f;

        int minutes = Mathf.FloorToInt(statsManager.stats.timeSurvived / 60f);
        int seconds = Mathf.FloorToInt(statsManager.stats.timeSurvived % 60f);

        finalStatsText.text = "<b>РЕЗУЛЬТАТЫ МАТЧА</b>\n\n" +
                              $"Вы продержались: <color=#FFD700>{minutes:00}:{seconds:00}</color>\n" +
                              $"Убито врагов: {statsManager.stats.killsCount}\n" +
                              $"Сделано выстрелов: {shots}\n" +
                              $"Попаданий по целям: {hits}\n" +
                              $"Точность стрельбы: {accuracy:F1}%\n" +
                              $"Нанесено урона: {statsManager.stats.totalDamageDealt:F0}";
    }

    public void ExitToMainMenu()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
        }
        SceneManager.LoadScene(mainMenuSceneName);
    }
}