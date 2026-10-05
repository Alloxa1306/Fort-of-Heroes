using UnityEngine;
using Unity.Entities;

public class PlayerTagComponent : MonoBehaviour, IConvertGameObjectToEntity
{
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        // Вешаем невидимую бирку "Игрок" на сущность в ECS
        dstManager.AddComponent<PlayerTag>(entity);
    }
}

// Сама пустая бирка для ECS
public struct PlayerTag : IComponentData { }