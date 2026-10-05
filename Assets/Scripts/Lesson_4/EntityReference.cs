using Unity.Entities;
using UnityEngine;

public class EntityReference : MonoBehaviour, IConvertGameObjectToEntity
{
    [HideInInspector] public Entity myEntity;
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        myEntity = entity; // Сохраняем ссылку на Entity
    }
}
