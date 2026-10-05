using Unity.Netcode;
using Unity.Entities;
using UnityEngine;

public class PlayerMoveSystem : ComponentSystem
{
    private EntityQuery _query;

    protected override void OnCreate()
    {
        _query = GetEntityQuery(
            ComponentType.ReadOnly<Transform>(),
            ComponentType.ReadOnly<PlayerMoveComponent>()
        );
    }

    protected override void OnUpdate()
    {

        float deltaTime = Time.DeltaTime;

        Entities.With(_query).ForEach((Entity entity, Transform transform, PlayerMoveComponent playerMove) =>
        {
            // Защита от дисконекта
            if (transform == null)
            {
                // Если GameObject удален Фотоном, убиваем связанную с ним Entity
                PostUpdateCommands.DestroyEntity(entity);
                return;
            }

            NetworkObject netObj = transform.GetComponent<NetworkObject>();
            if (netObj != null && !netObj.IsOwner) return;


            // Считаем перемещение персонажа
            Vector3 displacement = playerMove.direction * playerMove.moveSpeed * deltaTime;
            bool didDash = false;

            // Прижимаем персонажа к земеле
            displacement.y = -9.8f * deltaTime;

            // Обработка рывка
            if(playerMove.wantsToDash)
            {
                // VFX на месте старта
                if(playerMove.dashStartVfx != null)
                {
                    Object.Instantiate(playerMove.dashStartVfx,transform.position,Quaternion.identity);
                }

                // Вычисляем направление рывка
                Vector3 dashDirection = playerMove.direction.magnitude <0.1f ? transform.forward : playerMove.direction;

                Vector3 dashDisplacement = CombatRules.CalculateDashPosition(Vector3.zero, dashDirection, playerMove.dashDistanсe);

                // Добавляем дистанцию рывка
                displacement += dashDisplacement;

                playerMove.wantsToDash = false;
                didDash = true;
            }

            // Двигаем персонажа
            if(playerMove.characterController != null && playerMove.characterController.enabled)
            {
                playerMove.characterController.Move(displacement);
            }
            else
            {
                transform.position += displacement;
            }
            // VFX на финише рывка
            if(playerMove.dashEndVfx != null && didDash)
            {
                Object.Instantiate(playerMove.dashEndVfx,transform.position,Quaternion.identity);
            }

        });

    }
}