using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class MapInfo
{
    public string displayName;
    public string sceneName;
    public Sprite previewImage;

    [Header("Данные для панели статистики (только для UI)")]
    public Sprite[] monsterIcons;
    public float upgradeIntervalSeconds = 60f;
    public int bonusHealthPerWave = 20;
    public float bonusDamagePerWave = 5f;
}

public class MapSelectionUI : MonoBehaviour
{
    [Header("Доступные карты")]
    [SerializeField] private MapInfo[] maps;

    [Header("UI Префаб кнопки")]
    [SerializeField] private GameObject mapButtonPrefab;

    [Header("Контейнер для кнопок")]
    [SerializeField] private Transform buttonsParent;

    [Header("Ссылки")]
    [SerializeField] private RelayLauncher relayLauncher;
    [SerializeField] private ArenaInfoPanel arenaInfoPanel; // Ссылка на всплывающую панель

    private void OnEnable()
    {
        ClearButtons();
        CreateMapButtons();
    }

    private void ClearButtons()
    {
        foreach (Transform child in buttonsParent)
        {
            Destroy(child.gameObject);
        }
    }

    private void CreateMapButtons()
    {
        foreach (var map in maps)
        {
            GameObject buttonObj = Instantiate(mapButtonPrefab, buttonsParent);
            buttonObj.name = $"MapButton_{map.displayName}";

            // Ищем Image на самом объекте или на первом дочернем
            Image img = buttonObj.GetComponent<Image>();
            if (img == null) img = buttonObj.GetComponentInChildren<Image>(true);
            if (img != null && map.previewImage != null)
            {
                img.sprite = map.previewImage;
                img.type = Image.Type.Simple;
                img.preserveAspect = true;
            }

            // Ищем TMP_Text на самом объекте или на дочерних
            TMP_Text txt = buttonObj.GetComponentInChildren<TMP_Text>(true);
            if (txt != null)
            {
                txt.text = map.displayName;
            }

            // Настройка кнопки
            Button btn = buttonObj.GetComponent<Button>();
            if (btn != null)
            {
                MapInfo currentMap = map;

                btn.onClick.AddListener(() =>
                {
                    if (arenaInfoPanel != null)
                    {
                        // Открываем панель и передаем ей данные для отображения
                        arenaInfoPanel.OpenPanel(currentMap, relayLauncher);
                    }
                    else
                    {
                        Debug.LogError("Не назначена ссылка на ArenaInfoPanel в MapSelectionUI!");
                    }
                });
            }
        }
    }
}