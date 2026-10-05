using UnityEngine;
using UnityEngine.AI;
using Zenject;

public class EnemyComponent : MonoBehaviour
{
    [Header("Параметры врага (Внедряются через DI)")]
    public int health;
    public float moveSpeed;
    public float damage;
    public float attackRange;

    [Header("Для панели достижений")]
    public int monsterID;

    public Vector3 PatrolCenter { get; private set; }
    public float PatrolRadius { get; private set; }

    [Inject]
    public void Construct(IEnemyConfig config)
    {
        if (config != null)
        {
            // Если менеджер волн есть на сцене — применяем бусты к базовым значениям.
            if (WaveDifficultyManager.Instance != null)
            {
                health = WaveDifficultyManager.Instance.GetBoostedHealth(config.EnemyHealth);
                damage = WaveDifficultyManager.Instance.GetBoostedDamage(config.EnemyDamage);
            }
            else
            {
                // Фолбэк на случай тестирования префаба без менеджера
                health = config.EnemyHealth;
                damage = config.EnemyDamage;
            }

            moveSpeed = config.EnemySpeed;
            attackRange = config.EnemyAttackRange;
        }
    }

    void Start()
    {
        // Применяем настроенную скорость к агенту NavMesh
        if (TryGetComponent<NavMeshAgent>(out var agent))
        {
            agent.speed = moveSpeed;
        }
    }

    public void SetupPatrolArea(Vector3 center, float radius)
    {
        PatrolCenter = center;
        PatrolRadius = radius;
    }

    public class Factory : PlaceholderFactory<EnemyComponent> { }
}