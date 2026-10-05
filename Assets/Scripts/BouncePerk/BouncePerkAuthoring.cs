using UnityEngine;
using Unity.Entities;

public class BouncePerkAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new BouncePerkData { remainingTime = 0f });
    }
}
