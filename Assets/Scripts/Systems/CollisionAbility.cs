using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

// Список реакций при коллизии
public enum CollisionReaction
{
    None,           // Ничего не делаем
    DisableSelf,    // Выключить объект
    DestroySelf     // Уничтожаем себя
}

[RequireComponent(typeof(Collider))]
public class CollisionAbility : MonoBehaviour, IConvertGameObjectToEntity
{
    [Header("Настройки фильтра")]
    public List<string> targetTags = new List<string>();

    [Header("Реакция")]
    public CollisionReaction reaction = CollisionReaction.None;

    [Header("Действия")]
    public List<MonoBehaviour> collisionActions = new List<MonoBehaviour>();
    private List<IAbilityTarget> collisionActionsAbilities = new List<IAbilityTarget>();

    [HideInInspector] public List<Collider> collisions = new List<Collider>();
    
    private Entity myEntity;
    private EntityManager entityManager;

    private void Start()
    {
        // Проверяем реализет наш скрипт интерфейс
        foreach (var action in collisionActions)
        {
            if (action is IAbilityTarget ability)
                collisionActionsAbilities.Add(ability);
        }
    }

    public void Execute()
    {
        bool hasValidHit = false;

        foreach (var action in collisionActionsAbilities)
        {
            action.Targets.Clear(); // Очищаем старые цели

            foreach (var c in collisions)
            {
                if (c == null || c.gameObject == this.gameObject) continue;

                // Поиск по тегу
                if (targetTags.Count == 0 || targetTags.Contains(c.gameObject.tag))
                {
                    action.Targets.Add(c.gameObject);
                    hasValidHit = true; // Мы нашли подходящую цель!
                }
            }

            // Запускаем действие
            if (action.Targets.Count > 0)
            {
                action.Execute();
            }
        }

        // Применяем реакцию если нашли цель
        if (hasValidHit)
        {
            ApplyReaction();
        }
    }

    private void ApplyReaction()
    {
        switch (reaction)
        {
            case CollisionReaction.None: // Ничего не делаем
                break;

            case CollisionReaction.DisableSelf: // Выключаем себя
                gameObject.SetActive(false); 
                break;

            case CollisionReaction.DestroySelf: // Удаляем себя
                if (entityManager != default && myEntity != Entity.Null)
                {
                    entityManager.DestroyEntity(myEntity);
                }
                if (gameObject.GetComponent<BulletMoveComponent>() != null)
                {
                    BulletPool.Instance.ReturnBullet(gameObject);
                }
                else
                {
                    Destroy(gameObject);
                }
                break;
        }
    }

    public void InitializeDynamicEntity(Entity entity, EntityManager dstManager)
    {
        myEntity = entity;
        entityManager = dstManager;

        dstManager.AddComponentObject(entity, this);

        Collider myCollider = GetComponent<Collider>();
        if (myCollider == null) return;

        float3 position = gameObject.transform.position;

        switch (myCollider)
        {
            case SphereCollider sphere:
                sphere.ToWorldSpaceSphere(out var sphereCenter, out var sphereRadius);
                dstManager.AddComponentData(entity, new ActorColliderData { ColliderType = ColliderType.Sphere, SphereCenter = sphereCenter - position, SphereRadius = sphereRadius, initialTakeOff = true });
                break;
            case CapsuleCollider capsule:
                capsule.ToWorldSpaceCapsule(out var capsuleStart, out var capsuleEnd, out var capsuleRadius);
                dstManager.AddComponentData(entity, new ActorColliderData { ColliderType = ColliderType.Capsule, CapsuleStart = capsuleStart - position, CapsuleEnd = capsuleEnd - position, CapsuleRadius = capsuleRadius, initialTakeOff = true });
                break;
            case BoxCollider box:
                box.ToWorldSpaceBox(out var boxCenter, out var boxHalfExtents, out var boxOrientation);
                dstManager.AddComponentData(entity, new ActorColliderData { ColliderType = ColliderType.Box, BoxCenter = boxCenter - position, BoxHalfExtents = boxHalfExtents, BoxOrientation = boxOrientation, initialTakeOff = true });
                break;
        }
    }

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        InitializeDynamicEntity(entity, dstManager);
    }
}