using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class EnemyTagComponent : MonoBehaviour, IConvertGameObjectToEntity
{
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        // Вещаем бирку на врага
        dstManager.AddComponent<EnemyTag>(entity);
    }
}

public struct EnemyTag : IComponentData { }
