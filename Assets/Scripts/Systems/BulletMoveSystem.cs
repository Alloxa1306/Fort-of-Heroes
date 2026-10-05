using Unity.Entities;
using UnityEngine;

public class BulletMoveSystem : ComponentSystem
{
    private EntityQuery _query;

    protected override void OnCreate()
    {
        _query = GetEntityQuery(
            ComponentType.ReadOnly<Transform>(),
            ComponentType.ReadOnly<BulletMoveComponent>()
        );
    }

    protected override void OnUpdate()
    {
        float deltaTime = Time.DeltaTime;

        Entities.With(_query).ForEach((Entity entity, Transform transform, BulletMoveComponent bullet) =>
        {
            if (transform == null) return;

            float movementStep = bullet.speed * deltaTime;
            Vector3 direction = transform.forward;

            // Проверяем дистанцию перед пулей
            if (Physics.Raycast(transform.position, direction, out RaycastHit hit, movementStep + 0.15f))
            {
                // Проверка тега Wall на самом объекте и его родителе
                bool isWall = hit.collider.CompareTag("Wall") ||
                             (hit.collider.transform.parent != null && hit.collider.transform.parent.CompareTag("Wall"));

                if (isWall)
                {
                    if (bullet.canBounce)
                    {
                        // Вычисляем вектор отражения
                        Vector3 reflectedDirection = Vector3.Reflect(direction, hit.normal);
                        transform.rotation = Quaternion.LookRotation(reflectedDirection);

                        // Выталкиваем пулю из стены наружу по нормали, чтобы не зацепить стену на следующем кадре
                        transform.position = hit.point + hit.normal * 0.2f;

                        // Отключаем повторный рикошет
                        bullet.canBounce = false;

                        return;
                    }
                    else
                    {
                        if(bullet.explosionPrefab != null)
                        {
                            Object.Instantiate(bullet.explosionPrefab, hit.point,Quaternion.LookRotation(hit.normal));
                        }
                        // Пуля без рикошета (или уже отскочившая 1 раз) уничтожается о стену
                        DestroyBullet(transform, entity);
                        return;
                    }
                }
            }

            // Двигаем пулю вперед
            transform.position += transform.forward * movementStep;
            bullet.lifeTime -= deltaTime;

            if (bullet.lifeTime <= 0f)
            {
                DestroyBullet(transform, entity);
            }
        });
    }

    private void DestroyBullet(Transform transform, Entity entity)
    {
        // Обычное уничтожение GameObject, так как пуля локальная
        //Object.Destroy(transform.gameObject);
        //PostUpdateCommands.DestroyEntity(entity);
        BulletPool.Instance.ReturnBullet(transform.gameObject);

        PostUpdateCommands.DestroyEntity(entity);
    }
}