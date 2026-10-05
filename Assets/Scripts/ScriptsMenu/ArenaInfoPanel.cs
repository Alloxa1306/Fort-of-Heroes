using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class ArenaInfoPanel : MonoBehaviour
{
    [Header("UI Элементы")]
    public TextMeshProUGUI mapTitleText;
    public TextMeshProUGUI baseStatsText;
    public TextMeshProUGUI progressionText;
    public Image[] monsterSlots;

    private MapInfo selectedMap;
    private RelayLauncher currentRelayLauncher;

    public void OpenPanel(MapInfo mapInfo, RelayLauncher launcher)
    {
        selectedMap = mapInfo;
        currentRelayLauncher = launcher;

        mapTitleText.text = mapInfo.displayName;

        // Берем базу с Google Drive для текста
        if (GameDataManager.Instance != null && GameDataManager.Instance.EnemyConfig != null)
        {
            var config = GameDataManager.Instance.EnemyConfig;

            baseStatsText.text = $"<b>Базовые враги:</b>\n" +
                                 $"<color=#55FF55>Здоровье: {config.enemyHealth}</color> | " +
                                 $"<color=#FF5555>Урон: {config.enemyDamage}</color>";
        }
        else
        {
            baseStatsText.text = "Базовые характеристики загружаются...";
        }

        progressionText.text = $"<b>Прогрессия сложности:</b>\n" +
                               $"Каждые {mapInfo.upgradeIntervalSeconds} сек. монстры получают:\n" +
                               $"<color=#55FF55>+{mapInfo.bonusHealthPerWave} ХП</color> и <color=#FF5555>+{mapInfo.bonusDamagePerWave} урона</color>";

        // Заполняем иконки монстров
        for (int i = 0; i < monsterSlots.Length; i++)
        {
            if (i < mapInfo.monsterIcons.Length)
            {
                monsterSlots[i].sprite = mapInfo.monsterIcons[i];
                monsterSlots[i].gameObject.SetActive(true);
            }
            else
            {
                monsterSlots[i].gameObject.SetActive(false);
            }
        }

        gameObject.SetActive(true);
    }

    public void StartArena()
    {
        if (selectedMap == null) return;

        // Запускаем сетевой лончер
        if (currentRelayLauncher != null)
        {
            currentRelayLauncher.GamePlayScene = selectedMap.sceneName;
            currentRelayLauncher.CreateRelayGame();
        }
        else
        {
            SceneManager.LoadScene(selectedMap.sceneName);
        }
    }

    public void ClosePanel()
    {
        gameObject.SetActive(false);
    }
}