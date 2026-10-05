using UnityEngine;

public class PlayerStatsManager : MonoBehaviour
{
    public PlayerStats stats;
    public bool isPlayerAlive = true;

    private void Awake()
    {
        SyncProfile();
    }

    private void Update()
    {
        // Защита от асинхронной загрузки Google Drive
        if (GameDataManager.Instance != null && GameDataManager.Instance.isDataLoaded)
        {
            // Как только данные скачались, подхватываем настоящий профиль
            if (stats != GameDataManager.Instance.Profile)
            {
                stats = GameDataManager.Instance.Profile;
                ResetMatchStats(); // Сбрасываем убийства и время на актуальном профиле
            }
        }
    }

    private void SyncProfile()
    {
        if (GameDataManager.Instance != null && GameDataManager.Instance.Profile != null)
        {
            stats = GameDataManager.Instance.Profile;
        }
    }

    public void ResetMatchStats()
    {
        if (stats != null)
        {
            stats.timeSurvived = 0f;
            stats.killsCount = 0;
            stats.shotsCount = 0;
            stats.hitsCount = 0;
            stats.totalDamageDealt = 0;
        }
        if (stats.matchMonsterKillsByID != null)
        {
            System.Array.Clear(stats.matchMonsterKillsByID, 0, stats.matchMonsterKillsByID.Length);
        }
        isPlayerAlive = true;
    }

    public void StopTimer()
    {
        isPlayerAlive = false;
        Debug.Log($"[Stats] Таймер остановлен. Итоговое время: {stats?.timeSurvived} сек.");
    }

    public void SaveGlobalProgress()
    {
        if (GameDataManager.Instance != null)
        {
            // Вызываем правильный асинхронный метод из твоего GameDataManager 
            // Передаем true, чтобы синхронизировать данные с облаком
            _ = GameDataManager.Instance.SavePlayerProfile(true);

            Debug.Log("[Stats] Глобальная статистика отправлена на сохранение в Google Drive!");
        }
    }
}