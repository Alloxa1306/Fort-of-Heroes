using Unity.Entities;
using UnityEngine;
using UnityEngine.AI;

public class EnemyEntityRegistrar : MonoBehaviour
{
    public void RegisterEntity(Entity entity, EntityManager entityManager)
    {
        // Связываем базовые компоненты
        entityManager.AddComponentObject(entity, transform);

        entityManager.AddComponent<EnemyTag>(entity);

        if (TryGetComponent<Animator>(out var animator))
        {
            entityManager.AddComponentObject(entity, animator);
        }
        if (TryGetComponent<NavMeshAgent>(out var navMeshAgent))
        {
            entityManager.AddComponentObject(entity, navMeshAgent);
        }

        if (TryGetComponent<EnemyAnimationAuth>(out var animAuth))
        {
            entityManager.AddComponentObject(entity, animAuth);
        }

        if (TryGetComponent<EnemyComponent>(out var enemyComp))
        {
            entityManager.AddComponentObject(entity, enemyComp);
            // Добавляем HealthData, используя данные из компонента
            entityManager.AddComponentData(entity, new HealthData { Value = enemyComp.health });
        }

        // Связываем EntityReference для пуль
        if (TryGetComponent<EntityReference>(out var entityRef))
        {
            entityRef.myEntity = entity;
        }

        // Регистрируем ИИ
        if (TryGetComponent<BehaviourManager>(out var behaviourManager))
        {
            entityManager.AddComponentObject(entity, behaviourManager);
            entityManager.AddComponent<AIAgent>(entity);
        }

                                                                
        // Регистрируем коллайдер
        if (TryGetComponent<CollisionAbility>(out var collisionAbility))
        {
            collisionAbility.InitializeDynamicEntity(entity, entityManager);
        }
    }
}