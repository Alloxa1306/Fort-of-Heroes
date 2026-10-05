using UnityEngine;
using TMPro;

public class PlayerStatsUI : MonoBehaviour
{
    [Header("Компоненты данных")]
    [SerializeField] private PlayerStatsManager statsManager;

    [Header("Элементы интерфейса во время игры (HUD)")]
    [SerializeField] private GameObject hudPanel;          // Панель игрового интерфейса
    [SerializeField] private TextMeshProUGUI hudKillsText;  // Текст счетчика убийств
    [SerializeField] private TextMeshProUGUI hudTimeText;   // Текст времени жизни

    [Header("Элементы интерфейса Конца Игры (Game Over)")]
    [SerializeField] private GameObject gameOverPanel;     // Панель экрана смерти
    [SerializeField] private TextMeshProUGUI finalStatsText; // Поле для всей финальной статистики

    private bool isGameActive = true;

    void Start()
    {
        // На старте жестко включаем экран загрузки и прячем всё остальное
        if (hudPanel != null) hudPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        if (statsManager == null)
        {
            statsManager = FindObjectOfType<PlayerStatsManager>();
        }
    }

    void Update()
    {
        //Если игра окончена, таймер не тикает
        if(!isGameActive) return;

        if (statsManager != null)
        {
            //Обновляем время жизни игрока
            statsManager.stats.timeSurvived += Time.deltaTime;
            UpdateHUD();
        }
    }

    private void UpdateHUD()
    {
        if (hudKillsText != null)
        {
            hudKillsText.text = $"{statsManager.stats.killsCount}";
        }

        if (hudTimeText != null)
        {
            int minutes = Mathf.FloorToInt(statsManager.stats.timeSurvived / 60f);
            int seconds = Mathf.FloorToInt(statsManager.stats.timeSurvived % 60f);
            hudTimeText.text = $"{minutes:00}:{seconds:00}";
        }
    }

    // Метод вызывается из DeathSystem при смерти игрока
    public void TriggerGameOver()
    {
        isGameActive = false;

        if (hudPanel != null) hudPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(true);

        if (statsManager == null || finalStatsText == null) return;

        int shots = statsManager.stats.shotsCount;
        int hits = statsManager.stats.hitsCount;
        float accuracy = 0f;
        if (shots > 0)
        {
            accuracy = ((float)hits / shots) * 100f;
        }

        int minutes = Mathf.FloorToInt(statsManager.stats.timeSurvived / 60f);
        int seconds = Mathf.FloorToInt(statsManager.stats.timeSurvived % 60f);

        string statsSummary = "<b>РЕЗУЛЬТАТЫ МАТЧА</b>\n\n" +
                              $"Вы продержались: <color=#FFD700>{minutes:00}:{seconds:00}</color>\n" +
                              $"Убито врагов: {statsManager.stats.killsCount}\n" +
                              $"Сделано выстрелов: {shots}\n" +
                              $"Попаданий по целям: {hits}\n" +
                              $"Точность стрельбы: {accuracy:F1}%\n" +
                              $"Нанесено урона: {statsManager.stats.totalDamageDealt:F0}";

        finalStatsText.text = statsSummary;
    }
}