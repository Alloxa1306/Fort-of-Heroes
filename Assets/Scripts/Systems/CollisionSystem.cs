using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class CollisionSystem : ComponentSystem
{
    private EntityQuery collisionQuery;
    private Collider[] results = new Collider[50];

    protected override void OnCreate()
    {
        // Ищем Трансформы и Данные коллайдера
        collisionQuery = GetEntityQuery(
            ComponentType.ReadOnly<ActorColliderData>(),
            ComponentType.ReadOnly<Transform>(),
            ComponentType.ReadOnly<CollisionAbility>()
        );
    }

    protected override void OnUpdate()
    {
        Entities.With(collisionQuery).ForEach((Entity entity, Transform transform, ref ActorColliderData colliderData) =>
        {

            if (transform == null) return;

            // Быстрый поиск через Ecs память
            var abilityCollision = EntityManager.GetComponentObject<CollisionAbility>(entity);
            if (abilityCollision == null) return;

            var gameObject = transform.gameObject;
            float3 position = gameObject.transform.position;
            Quaternion rotation = gameObject.transform.rotation;

            int size = 0;

            // Определяем коллайдеры
            switch (colliderData.ColliderType)
            {
                case ColliderType.Sphere:
                    size = Physics.OverlapSphereNonAlloc(colliderData.SphereCenter + position, colliderData.SphereRadius, results);
                    break;
                case ColliderType.Capsule:
                    var center = (float3)((colliderData.CapsuleStart + position) + (colliderData.CapsuleEnd + position)) / 2f;
                    var point1 = colliderData.CapsuleStart + position;
                    var point2 = colliderData.CapsuleEnd + position;
                    point1 = (float3)(rotation * (point1 - center)) + center;
                    point2 = (float3)(rotation * (point2 - center)) + center;
                    size = Physics.OverlapCapsuleNonAlloc(point1, point2, colliderData.CapsuleRadius, results);
                    break;
                case ColliderType.Box:
                    size = Physics.OverlapBoxNonAlloc(colliderData.BoxCenter + position, colliderData.BoxHalfExtents, results, colliderData.BoxOrientation * rotation);
                    break;
            }

            // Очищаем список коллизий прошлого кадра
            abilityCollision.collisions.Clear();

            if (size > 0)
            {
                // Заполняем список новыми столкновениями
                for (int i = 0; i < size; i++)
                {
                    abilityCollision.collisions.Add(results[i]);
                }

                // Запускаем все Execute
                abilityCollision.Execute();
            }
        });
    }
}