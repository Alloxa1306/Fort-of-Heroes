using Unity.Entities;
using UnityEngine;

[RequireComponent(typeof(EnemyComponent))]
public class EnemyEntityHealth : MonoBehaviour, IConvertGameObjectToEntity
{
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        EnemyComponent enemyData = GetComponent<EnemyComponent>();
        int health = (enemyData != null) ? enemyData.health : 100;

        dstManager.AddComponentData(entity, new HealthData { Value = health });
        Debug.Log($"[ECS КОНВЕРСИЯ ВРАГА]: Сущности {gameObject.name} присвоено {health} ХП из EnemyComponent");
    }
}