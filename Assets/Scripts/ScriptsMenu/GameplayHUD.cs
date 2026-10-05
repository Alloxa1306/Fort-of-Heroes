using UnityEngine;
using TMPro;

[System.Serializable]
public class HUDMonsterSlot
{
    [Tooltip("ID монстра из EnemyComponent")]
    public int monsterID;
    public TextMeshProUGUI killsText;
}

public class GameplayHUD : MonoBehaviour
{
    [Header("Компоненты текста")]
    [SerializeField] private TextMeshProUGUI coinText;
    [SerializeField] private TextMeshProUGUI timeText;

    [Header("Счетчики убийств по видам монстров")]
    [SerializeField] private HUDMonsterSlot[] monsterSlots;

    private PlayerStatsManager statsManager;
    private bool isTimerActive = false;

    private void Start()
    {
        statsManager = Object.FindAnyObjectByType<PlayerStatsManager>();

        if (statsManager != null)
        {
            statsManager.ResetMatchStats();
        }

        InitializeHUD();
    }

    public void InitializeHUD()
    {
        if (statsManager != null)
        {
            UpdateCoinText(statsManager.stats.coinsCount);

            if (statsManager.stats.matchMonsterKillsByID != null)
            {
                foreach (var slot in monsterSlots)
                {
                    int mId = slot.monsterID;
                    if (mId >= 0 && mId < statsManager.stats.matchMonsterKillsByID.Length)
                    {
                        if (slot.killsText != null)
                        {
                            slot.killsText.text = statsManager.stats.matchMonsterKillsByID[mId].ToString();
                        }
                    }
                }
            }
        }
    }

    public void StartTimer() => isTimerActive = true;
    public void StopTimer() => isTimerActive = false;

    private void Update()
    {
        if (isTimerActive && statsManager != null)
        {
            statsManager.stats.timeSurvived += Time.deltaTime;
            UpdateTimeText(statsManager.stats.timeSurvived);
        }
    }

    public void UpdateCoinText(int currentCoins)
    {
        if (coinText != null) coinText.text = currentCoins.ToString();
    }

    private void UpdateTimeText(float timeSurvived)
    {
        if (timeText != null)
        {
            int minutes = Mathf.FloorToInt(timeSurvived / 60f);
            int seconds = Mathf.FloorToInt(timeSurvived % 60f);
            timeText.text = $"{minutes:00}:{seconds:00}";
        }
    }
}