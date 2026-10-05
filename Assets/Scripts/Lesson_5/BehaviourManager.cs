using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using DefaultNamespace.Components.Interfaces;

public class BehaviourManager : MonoBehaviour, IConvertGameObjectToEntity
{
    [Header("Список MonoBehaviour-компонентов поведений")]
    public List<MonoBehaviour> behaviours;

    [HideInInspector] public IBehaviour activeBehaviour;
    [HideInInspector] public IBehaviour[] cachedBehaviours;

    void Start()
    {
        if (behaviours != null)
        {
            cachedBehaviours = new IBehaviour[behaviours.Count];

            for (int i = 0; i < behaviours.Count; i++)
            {
                cachedBehaviours[i] = behaviours[i] as IBehaviour;
            }
        }
    }

    private void OnDestroy()
    {
        cachedBehaviours = null;
        activeBehaviour = null;
    }

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponent<AIAgent>(entity);
    }
}

public struct AIAgent : IComponentData { }
