using Unity.Entities;
using UnityEngine;
using Unity.Netcode;

public class PlayerShootSystem : ComponentSystem
{
    private EntityQuery _query;

    protected override void OnCreate()
    {
        _query = GetEntityQuery(
            ComponentType.ReadOnly<Transform>(),
            ComponentType.ReadOnly<PlayerWeaponComponent>()
        );
    }

    protected override void OnUpdate()
    {
        Entities.With(_query).ForEach((Entity entity, Transform transform, PlayerWeaponComponent weapon) =>
        {
            if (transform == null)
            {
                PostUpdateCommands.DestroyEntity(entity);
                return;
            }

            NetworkObject netObj = transform.GetComponent<NetworkObject>();
            if (netObj != null && netObj.IsOwner)
            {
                if (weapon.wantsToShoot)
                {
                    if (UnityEngine.Time.time >= weapon.nextFireTime)
                    {
                        if (weapon.bulletPrefab != null && weapon.firePoint != null)
                        {
                            // Проверяем наличие перка отскока перед выстрелом
                            bool hasBounce = false;

                            Entity targetPlayerEntity = entity;
                            if (transform.TryGetComponent<EntityReference>(out var entRef) && entRef.myEntity != Entity.Null)
                            {
                                targetPlayerEntity = entRef.myEntity;
                            }

                            if (EntityManager.HasComponent<BouncePerkData>(targetPlayerEntity))
                            {
                                var perkData = EntityManager.GetComponentData<BouncePerkData>(targetPlayerEntity);
                                hasBounce = perkData.remainingTime > 0f;
                            }
                            else if (EntityManager.HasComponent<BouncePerkData>(entity))
                            {
                                var perkData = EntityManager.GetComponentData<BouncePerkData>(entity);
                                hasBounce = perkData.remainingTime > 0f;
                            }

                            //  Вызываем скрипт отправки RPC, передавая информацию о перке!
                            var weaponRPC = transform.GetComponent<PlayerWeaponRPC>();
                            if (weaponRPC != null)
                            {
                                weaponRPC.FireWeapon(weapon.firePoint.position, weapon.firePoint.rotation, hasBounce);
                            }

                            // Записываем статистику
                            var statsManager = Object.FindAnyObjectByType<PlayerStatsManager>();
                            if (statsManager != null)
                            {
                                statsManager.stats.shotsCount++;
                            }

                            weapon.nextFireTime = UnityEngine.Time.time + weapon.fireRate;
                        }
                    }

                    weapon.wantsToShoot = false;
                }
            }
        });
    }
}