using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class DeathSystem : ComponentSystem
{

    private EntityQuery query;

    private GameDataManager dataManager;
    private PlayerStatsManager playerStatsManager;
    private GameplayUIManager gameplayUIManager;
    private GameplayHUD gameplayHUD;

    // Параметры Теста смерти
    //private float debugTimer = 0f;
    //private bool debugPlayerKilled = false;

    protected override void OnCreate()
    {
        query = GetEntityQuery(
            ComponentType.ReadOnly<HealthData>(),
            ComponentType.ReadOnly<Transform>()
            );
    }


    protected override void OnUpdate()
    {

        if (dataManager == null) dataManager = Object.FindObjectOfType<GameDataManager>();
        if (playerStatsManager == null) playerStatsManager = Object.FindObjectOfType<PlayerStatsManager>();
        if (gameplayUIManager == null) gameplayUIManager = Object.FindObjectOfType<GameplayUIManager>();
        if (gameplayHUD == null) gameplayHUD = Object.FindObjectOfType<GameplayHUD>(); ;

        Entities.With(query).ForEach((Entity entity, Transform transform, ref HealthData health) =>
        {

            if (transform == null) return;


            // Проверка здоровья 
            if (health.Value <= 0)
            {
                // 1. Сразу замораживаем гравитацию для всех, чтобы тела не падали сквозь пол
                var rb = transform.GetComponent<Rigidbody>();
                if (rb != null) rb.isKinematic = true;

                // Получаем коллайдер один раз, чтобы избежать ошибки CS0136
                var myCollider = transform.GetComponent<Collider>();

                // 2. РАЗДЕЛЯЕМ ЛОГИКУ КОЛЛАЙДЕРОВ
                if (EntityManager.HasComponent<PlayerTag>(entity))
                {
                    // Логика смерти игрока
                    if (playerStatsManager != null) playerStatsManager.StopTimer();
                    if (dataManager != null) _ = dataManager.SavePlayerProfile(true);
                    if (gameplayUIManager != null) gameplayUIManager.TriggerGameOver();

                    // Игроку делаем коллайдер триггером (он остается на месте для камеры и интерфейса, но не мешает физике)
                    if (myCollider != null) myCollider.isTrigger = true;
                }
                else if (EntityManager.HasComponent<EnemyTag>(entity))
                {
                    // Логика смерти монстра
                    var loot = transform.GetComponent<EnemyLootDrop>();
                    if (loot != null) loot.DropLoot();

                    // ПОЛНОСТЬЮ отключаем коллайдер монстра. 
                    // Пули будут пролетать сквозь труп, ApplyDamage не сработает!
                    if (myCollider != null) myCollider.enabled = false;
                }

                // Запуск анимации смерти
                var animator = transform.GetComponent<Animator>();
                if (animator != null)
                {
                    int shootLayerIndex = animator.GetLayerIndex("ShootLayer");
                    if (shootLayerIndex != -1) animator.SetLayerWeight(shootLayerIndex, 0f);

                    // Используем хеш для смерти
                    if (EntityManager.HasComponent<EnemyAnimationAuth>(entity))
                    {
                        var animAuth = EntityManager.GetComponentObject<EnemyAnimationAuth>(entity);
                        animator.SetTrigger(animAuth.DieHash);
                    }
                    else
                    {
                        animator.SetTrigger("Die");
                    }
                }

                // Отключаем физику
                var input = transform.GetComponent<PlayerInputHandler>();
                if (input != null)
                {
                    input.enabled = false;
                }


                var navMeshAgent = transform.GetComponent<UnityEngine.AI.NavMeshAgent>();

                if (navMeshAgent != null && navMeshAgent.isOnNavMesh)
                {
                    navMeshAgent.isStopped = true;
                    navMeshAgent.enabled = false;
                }

                var brain = transform.GetComponent<BehaviourManager>();
                if (brain != null)
                {
                    brain.enabled = false;
                }

                var netObj = transform.GetComponent<NetworkObject>();

                if (netObj != null)
                {
                    if (NetworkManager.Singleton.IsServer)
                    {
                        Object.Destroy(transform.gameObject, 2f);
                    }
                }
                else
                {
                    Object.Destroy(transform.gameObject, 2f);
                }

                // Уничтожаем сущность
                PostUpdateCommands.DestroyEntity(entity);
            }
        });
    }
}