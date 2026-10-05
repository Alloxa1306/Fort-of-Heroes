using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class ApplyBouncePerk : MonoBehaviour, IAbilityTarget
{
    public List<GameObject> Targets { get; set; } = new List<GameObject>(); // Поиск целей по таргету

    [Header("Настройки времени перка")]
    public float perkDuration = 10f;

    public void Execute()
    {
        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        foreach(var target in Targets)
        {
            if (target == this.gameObject) continue;

            var entityRef = target.GetComponent<EntityReference>(); 

            // Привязка к сущности перка отскока
            if(entityRef != null && entityRef.myEntity != Entity.Null)
            
            {
                var perkData = entityManager.GetComponentData<BouncePerkData>(entityRef.myEntity);

                perkData.remainingTime = perkDuration;

                entityManager.SetComponentData(entityRef.myEntity, perkData);

                Debug.Log($" [{target.name}] подобрал перк отскока снарядов!");
            }
        }
    }
}
