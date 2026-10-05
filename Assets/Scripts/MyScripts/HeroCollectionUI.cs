using System;
using UnityEngine;

public class HeroCollectionUI : MonoBehaviour
{
    [Header("Кнопка Назад")]
    [SerializeField] private GameObject mainPanel;

    public event Action OnCollectionUpdated;

    private void OnEnable()
    {
        OnCollectionUpdated?.Invoke();
    }

    public void SelectHero(string heroId)
    {
        if (GameDataManager.Instance == null || GameDataManager.Instance.Profile == null) return;

        // Мгновенно обновляем данные в памяти
        GameDataManager.Instance.Profile.selectedCharacterId = heroId;

        // Мгновенно обновляем UI карточек 
        OnCollectionUpdated?.Invoke();

        Debug.Log($"[Collection] Выбран герой: {heroId}");

        // Отправляем сохранение на Google Диск в ФОНОВОМ режиме 
        _ = SaveInBackground();
    }

    private async System.Threading.Tasks.Task SaveInBackground()
    {
        if (GameDataManager.Instance != null)
        {
            // Сохраняем в JSON локально и отправляем в облако фоновой задачей
            await GameDataManager.Instance.SavePlayerProfile(true);
        }
    }

    public void OnClickBack()
    {
        UIManager mainMenuUi  = FindObjectOfType<UIManager>();
        if (mainMenuUi != null)
        {
            mainMenuUi.GoBack();
            return;
        }

        GameplayUIManager gameplayUIManager = FindObjectOfType<GameplayUIManager>();
        if (gameplayUIManager != null)
        {
            gameplayUIManager.GoBack();
            return;
        }

        gameObject.SetActive(false); // Если менеджер не найден
    }
}