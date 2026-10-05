using System;
using UnityEngine;
using Unity.Netcode;
using Zenject;
using Unity.Entities;
using Cinemachine;

public class PlayerSpawner : NetworkBehaviour
{
    public static event Action<GameObject> OnLocalPlayerSpawned;
    public static GameObject LocalPlayerObject { get; private set; }

    [SerializeField] private HeroConfig[] allHeroes;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private CinemachineVirtualCamera virtualCamera;

    [Inject] private DiContainer container;
    [Inject] private IPlayerConfig googlePlayerConfig;

    public override void OnNetworkSpawn()
    {
        if (IsClient)
        {
            string selectedId = GameDataManager.Instance?.Profile?.selectedCharacterId ?? "default_hero";
            RequestSpawnServerRpc(selectedId, NetworkManager.Singleton.LocalClientId);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestSpawnServerRpc(string heroId, ulong clientId)
    {
        HeroConfig config = System.Array.Find(allHeroes, h => h.heroId == heroId) ?? allHeroes[0];

        Vector3 spawnPosition = Vector3.zero;
        Quaternion spawnRotation = Quaternion.identity;

        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            int index = (int)(clientId % (ulong)spawnPoints.Length);
            if (spawnPoints[index] != null)
            {
                spawnPosition = spawnPoints[index].position;
                spawnRotation = spawnPoints[index].rotation;
            }
        }

        // Создаем объект на сервере 
        GameObject spawnedPlayer = Instantiate(config.heroPrefab, spawnPosition, spawnRotation);

        // Спавним его в сеть и передаем права управления клиенту
        NetworkObject netObj = spawnedPlayer.GetComponent<NetworkObject>();
        netObj.SpawnAsPlayerObject(clientId, true);

    }

    public void SetupPlayerLocally(NetworkObject netObj)
    {
        GameObject spawnedPlayer = netObj.gameObject;

        // Внедряем зависимости Zenject
        container.InjectGameObject(spawnedPlayer);

        if (spawnedPlayer.TryGetComponent<PlayerComponent>(out var playerComp))
        {
            playerComp.Construct(googlePlayerConfig);
        }

        // Регистрируем в ECS
        RegisterPlayerInECS(spawnedPlayer);

        if (netObj.IsOwner)
        {
            if (virtualCamera != null)
            {
                virtualCamera.Follow = spawnedPlayer.transform;
            }

            LocalPlayerObject = spawnedPlayer;
            OnLocalPlayerSpawned?.Invoke(spawnedPlayer);
        }
    }

    private void RegisterPlayerInECS(GameObject playerObject)
    {
        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        Entity playerEntity = entityManager.CreateEntity();

        entityManager.AddComponentObject(playerEntity, playerObject.transform);

        if (playerObject.TryGetComponent<PlayerMoveComponent>(out var moveComp))
            entityManager.AddComponentObject(playerEntity, moveComp);

        if (playerObject.TryGetComponent<PlayerWeaponComponent>(out var weaponComp))
            entityManager.AddComponentObject(playerEntity, weaponComp);

        if (playerObject.TryGetComponent<PlayerComponent>(out var playerComp))
        {
            entityManager.AddComponentData(playerEntity, new HealthData
            {
                Value = playerComp.health,
            });
        }

        // Регистрируем тег игрока, чтобы ИИ видел клона 
        entityManager.AddComponentData(playerEntity, new PlayerTag());

        if (playerObject.TryGetComponent<EntityReference>(out var entityRef))
        {
            entityRef.myEntity = playerEntity;
        }
        else
        {
            entityRef = playerObject.AddComponent<EntityReference>();
            entityRef.myEntity = playerEntity;
        }
    }
}