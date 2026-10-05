using Unity.Entities;
using UnityEngine;
using Zenject;

public class EntityHealth : MonoBehaviour, IConvertGameObjectToEntity
{
    public enum EntityType
    {
        Player,
        Enemy
    }
    [Header("Тип этой сущности")]
    public EntityType targetType;

    private IPlayerConfig _config;

    [Inject]
    public void Construct(IPlayerConfig config)
    {
        _config = config;
    }

    [HideInInspector] public int startHealth;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        if (_config != null)
        {
            switch (targetType)
            {
                case EntityType.Player:
                    startHealth = _config.HeroHealth;
                    break;
                //case EntityType.Enemy:
                //    startHealth = _config.EnemyHealth;
                //    break;
                default:
                    startHealth = 100;
                    break;
            }
        }

        else
        {
            startHealth = 100; // Заглушка
        }
        dstManager.AddComponentData(entity, new HealthData { Value = startHealth }); // Регестрируем здоровье в ECS
    }
}