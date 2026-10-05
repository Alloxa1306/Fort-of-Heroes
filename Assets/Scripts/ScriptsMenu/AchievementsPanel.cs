using UnityEngine;
using TMPro;
using UnityEngine.UI;

[System.Serializable]
public class MonsterAchievementSlot
{
    public int monsterID;
    public string monsterName;
    public Sprite monsterIcon;
}

public class AchievementsPanel : MonoBehaviour
{
    [Header("Общая статистика")]
    public TextMeshProUGUI totalKillsText;
    public TextMeshProUGUI totalCoinsText;

    [Header("Настройка монстров")]
    public MonsterAchievementSlot[] trackedMonsters;

    [Header("UI Префаб и Контейнер")]
    public GameObject achievementPrefab;
    public Transform achievementsContainer;

    private void OnEnable()
    {
        UpdateAchievementsUI();
    }

    private void UpdateAchievementsUI()
    {
        // Очищаем старые записи
        foreach (Transform child in achievementsContainer)
        {
            Destroy(child.gameObject);
        }

        // Защита от пустых данных
        if (GameDataManager.Instance == null || GameDataManager.Instance.Profile == null) return;

        var profile = GameDataManager.Instance.Profile;

        // Выводим общие цифры
        if (totalKillsText != null) totalKillsText.text = $"Всего убито: {profile.globalTotalKills}";
        if (totalCoinsText != null) totalCoinsText.text = $"Всего заработано: {profile.globalTotalCoinsEarned}";

        //  Генерируем список по конкретным монстрам
        foreach (var monsterData in trackedMonsters)
        {
            GameObject slotObj = Instantiate(achievementPrefab, achievementsContainer);

            // Ищем компоненты в префабе
            Image icon = slotObj.GetComponentInChildren<Image>();
            TextMeshProUGUI statsText = slotObj.GetComponentInChildren<TextMeshProUGUI>();

            if (icon != null) icon.sprite = monsterData.monsterIcon;

            if (statsText != null)
            {
                // Достаем количество убийств конкретного монстра по его ID 
                int kills = profile.monsterKillsByID[monsterData.monsterID];
                statsText.text = $"{monsterData.monsterName}\n<color=#FF5555>Убито: {kills}</color>";
            }
        }
    }
}