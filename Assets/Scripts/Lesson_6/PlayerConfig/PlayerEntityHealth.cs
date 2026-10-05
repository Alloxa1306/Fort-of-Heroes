using Unity.Entities;
using UnityEngine;

[RequireComponent(typeof(PlayerComponent))]
public class PlayerEntityHealth : MonoBehaviour, IConvertGameObjectToEntity
{
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        PlayerComponent playerData = GetComponent<PlayerComponent>();
        int health = (playerData != null) ? playerData.health : 100;

        // Регистрируем здоровье в ECS
        dstManager.AddComponentData(entity, new HealthData { Value = health });
    }
}