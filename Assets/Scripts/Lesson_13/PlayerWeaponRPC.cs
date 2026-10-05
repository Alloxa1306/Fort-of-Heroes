using Unity.Netcode;
using UnityEngine;
using Unity.Entities;
using FMODUnity;

public class PlayerWeaponRPC : NetworkBehaviour
{
    [Header("Звуки")]
    public EventReference shootSound;

    public void FireWeapon(Vector3 position, Quaternion rotation, bool canBounce)
    {
        if (IsOwner)
        {
            // Передаем серверу наш локальный ID игрока
            ShootServerRpc(position, rotation, canBounce, NetworkManager.Singleton.LocalClientId);
            RuntimeManager.PlayOneShot(shootSound,transform.position);
        }
    }

    [ServerRpc(RequireOwnership = true)]
    private void ShootServerRpc(Vector3 position, Quaternion rotation, bool canBounce, ulong shooterId)
    {
        // Рассылаем этот ID всем клиентам
        ShootClientRpc(position, rotation, canBounce, shooterId);
    }

    [ClientRpc]
    private void ShootClientRpc(Vector3 position, Quaternion rotation, bool canBounce, ulong shooterId)
    {
        GameObject instance = BulletPool.Instance.GetBullet(position, rotation);

        if (instance.TryGetComponent<ApplyDamage>(out var damageComp))
        {
            damageComp.isLocalBullet = (shooterId == NetworkManager.Singleton.LocalClientId);
        }

        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        Entity bulletEntity = entityManager.CreateEntity();

        entityManager.AddComponentObject(bulletEntity, instance.transform);

        if (instance.TryGetComponent<CollisionAbility>(out var collisionAbility))
        {
            collisionAbility.InitializeDynamicEntity(bulletEntity, entityManager);
        }

        var bulletMove = instance.GetComponent<BulletMoveComponent>();
        if (bulletMove != null)
        {
            bulletMove.canBounce = canBounce;
            bulletMove.lifeTime = 2f;
            entityManager.AddComponentObject(bulletEntity, bulletMove);
        }

        if (!instance.TryGetComponent<EntityReference>(out var entityRef))
        {
            entityRef = instance.AddComponent<EntityReference>();
        }
        entityRef.myEntity = bulletEntity;
    }
}