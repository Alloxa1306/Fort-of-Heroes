using Unity.Netcode;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UnityEngine.AI;
using Zenject;

public class EnemySpawner : NetworkBehaviour
{
    public enum SpawnMode
    {
        AreaAroundSpawner,
        SpecificPoints
    }

    [Header("Режим спавна")]
    [SerializeField] private SpawnMode currentSpawnMode = SpawnMode.AreaAroundSpawner;

    [Header("Параметры спавна")]
    [SerializeField] private int countGolem = 3;
    [SerializeField] private float spawnInterval = 5f;
    [SerializeField] private GameObject enemyPrefab;

    [Header("Настройки для AreaAroundSpawner")]
    [SerializeField] private float spawnRadius = 10f;

    [Header("Настройки для SpecificPoints")]
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private Transform patrolPoint;
    [SerializeField] private float patrolRadius = 5f;

    private float timer;
    private List<EnemyComponent> _activeEnemies = new List<EnemyComponent>();

    [Inject] private DiContainer _container;

    void Update()
    {
        if (!IsServer) return;

        _activeEnemies.RemoveAll(enemy => enemy == null);

        if (_activeEnemies.Count < countGolem)
        {
            timer += Time.deltaTime;
            if (timer >= spawnInterval)
            {
                timer = 0f;
                SpawnEnemy();
            }
        }
    }

    private void SpawnEnemy()
    {
        if (enemyPrefab == null) return;

        Vector3 spawnPosition = transform.position;

        if (currentSpawnMode == SpawnMode.AreaAroundSpawner)
        {
            spawnPosition = GetRandomPointInCircle(transform.position, spawnRadius);
        }
        else if (currentSpawnMode == SpawnMode.SpecificPoints && spawnPoints != null && spawnPoints.Length > 0)
        {
            Transform randomPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            spawnPosition = randomPoint.position;
        }

        GameObject enemyInstance = Instantiate(enemyPrefab, spawnPosition, Quaternion.Euler(0, Random.Range(0f, 360f), 0));
        enemyInstance.GetComponent<NetworkObject>().Spawn();
    }

    public void InitEnemy(GameObject enemyObj, bool isServer)
    {
        if (_container != null)
        {
            _container.InjectGameObject(enemyObj);
        }
        else
        {
            Debug.LogError("[EnemySpawner] Ошибка: DiContainer не найден!");
        }

        var world = World.DefaultGameObjectInjectionWorld;
        if (world == null) return;
        var entityManager = world.EntityManager;

        EnemyComponent newEnemy = enemyObj.GetComponent<EnemyComponent>();
        if (newEnemy != null)
        {
            if (isServer)
            {
                Vector3 patrolCenter = (currentSpawnMode == SpawnMode.SpecificPoints && patrolPoint != null)
                    ? patrolPoint.position
                    : enemyObj.transform.position;

                float radius = currentSpawnMode == SpawnMode.SpecificPoints ? patrolRadius : spawnRadius;
                newEnemy.SetupPatrolArea(patrolCenter, radius);
                _activeEnemies.Add(newEnemy);
            }
        }

        Entity enemyEntity = entityManager.CreateEntity();

        if (enemyObj.TryGetComponent<EnemyEntityRegistrar>(out var registrar))
        {
            registrar.RegisterEntity(enemyEntity, entityManager);
        }

        if (!isServer)
        {
            if (enemyObj.TryGetComponent<NavMeshAgent>(out var agent)) agent.enabled = false;
            if (enemyObj.TryGetComponent<BehaviourManager>(out var brain)) brain.enabled = false;

            if (entityManager.HasComponent<AIAgent>(enemyEntity))
            {
                entityManager.RemoveComponent<AIAgent>(enemyEntity);
            }
        }
    }

    private Vector3 GetRandomPointInCircle(Vector3 center, float radius)
    {
        Vector2 randomCirclePoint = Random.insideUnitCircle * radius;
        Vector3 randomTargetPos = new Vector3(center.x + randomCirclePoint.x, center.y, center.z + randomCirclePoint.y);

        if (NavMesh.SamplePosition(randomTargetPos, out NavMeshHit hit, radius, NavMesh.AllAreas))
        {
            return hit.position;
        }
        return center;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        if (spawnPoints != null)
        {
            foreach (var point in spawnPoints)
            {
                if (point != null) Gizmos.DrawSphere(point.position, 0.5f);
            }
        }

        if (currentSpawnMode == SpawnMode.AreaAroundSpawner)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, spawnRadius);
        }
        else if (currentSpawnMode == SpawnMode.SpecificPoints && patrolPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(patrolPoint.position, 0.5f);

            Gizmos.DrawWireSphere(patrolPoint.position, patrolRadius);
        }
    }
}
