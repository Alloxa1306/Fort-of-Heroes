using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;

public class PlayerHUDDisplay : MonoBehaviour
{
    [Header("Ссылки на компоненты")]
    [SerializeField] private PlayerComponent player;
    [SerializeField] private CharacterEffectController effectController;

    [Header("1. Индикатор здоровья (HP)")]
    [SerializeField] private Slider hpSlider;
    [SerializeField] private TMP_Text hpText;

    [Header("2. Характеристики игрока")]
    [SerializeField] private TMP_Text speedText;
    [SerializeField] private TMP_Text fireRateText;

    [Header("3. Активные эффекты")]
    [SerializeField] private GameObject shieldIndicator;
    [SerializeField] private TMP_Text shieldStatusText;

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
        player = playerObject.GetComponent<PlayerComponent>();
        effectController = playerObject.GetComponent<CharacterEffectController>();
    }

    private void Update()
    {
        if (player == null) return;

        UpdateHealthUI();
        UpdateStatsUI();
        UpdateEffectsUI();
    }


    private void UpdateHealthUI()
    {
        int maxHealth = Mathf.Max(1, player.maxHealth);
        int currentHealth = Mathf.Clamp(player.health, 0, maxHealth);

        if (hpSlider != null)
        {
            hpSlider.maxValue = maxHealth;
            hpSlider.value = currentHealth;
        }

        if (hpText != null)
        {
            hpText.text = $"{currentHealth} / {maxHealth}";
        }
    }

    private void UpdateStatsUI()
    {
        // Обновление текущей скорости
        if (speedText != null)
        {
            speedText.text = $"Скорость: {player.moveSpeed:F1}";
        }

        // Обновление темпа стрельбы
        if (fireRateText != null)
        {
            fireRateText.text = $"Темп стрельбы: {player.fireRate:F2}с";
        }
    }

    private void UpdateEffectsUI()
    {
        bool isShieldActive = effectController != null && effectController.IsShieldActive;

        // Включение/отключение иконки щита на UI
        if (shieldIndicator != null)
        {
            shieldIndicator.SetActive(isShieldActive);
        }

        if (shieldStatusText != null)
        {
            shieldStatusText.text = isShieldActive ? "Щит: Активен" : "Щит: Нет";
        }
    }
}