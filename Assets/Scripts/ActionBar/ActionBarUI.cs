using UnityEngine;

public class ActionBarUI : MonoBehaviour
{
    [Header("Кнопки на экране (Перетащи сюда QuickSlotButton)")]
    [SerializeField] private QuickSlotButton[] actionButtons;

    private void Start()
    {
        PlayerSpawner.OnLocalPlayerSpawned += HandlePlayerSpawned;
        if (PlayerSpawner.LocalPlayerObject != null)
        {
            HandlePlayerSpawned(PlayerSpawner.LocalPlayerObject);
        }
    }

    private void OnDestroy()
    {
        PlayerSpawner.OnLocalPlayerSpawned -= HandlePlayerSpawned;
    }

    private void HandlePlayerSpawned(GameObject playerObject)
    {
        // Ищем на заспавненном игроке систему QuickSlots
        if (playerObject.TryGetComponent<PlayerQuickSlots>(out var quickSlots))
        {
            // Передаем ему наши кнопки с экрана
            quickSlots.skillButtons = actionButtons;
        }
    }
}