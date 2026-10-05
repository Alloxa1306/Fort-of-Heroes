using UnityEngine;

public class WaveDifficultyManager : MonoBehaviour
{
    public static WaveDifficultyManager Instance { get; private set; }

    [Header("Настройки волн")]
    [Tooltip("Время в секундах до усиления (например, 60 = 1 минута)")]
    public float timeToUpgrade = 60f;

    [Header("Бонусы за каждую волну")]
    public int bonusHealth = 20;
    public float bonusDamage = 5f;

    public float SceneTimer { get; private set; } = 0f;
    public int CurrentWaveLevel { get; private set; } = 0;

    private void Awake()
    {
        Instance = this;
        SceneTimer = 0f;
        CurrentWaveLevel = 0;
    }

    private void Update()
    {
        SceneTimer += Time.deltaTime;

        // Рассчитываем, сколько полных интервалов времени прошло
        int calculatedLevel = Mathf.FloorToInt(SceneTimer / timeToUpgrade);

        if (calculatedLevel > CurrentWaveLevel)
        {
            CurrentWaveLevel = calculatedLevel;
            Debug.Log($"[Волна {CurrentWaveLevel}] Враги усилены! Прошло: {SceneTimer} сек.");
        }
    }

    // Методы для пересчета статов
    public int GetBoostedHealth(int baseHealth) => baseHealth + (CurrentWaveLevel * bonusHealth);
    public float GetBoostedDamage(float baseDamage) => baseDamage + (CurrentWaveLevel * bonusDamage);
}