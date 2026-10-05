using UnityEngine;
using Unity.Netcode;
using Unity.Entities;

[RequireComponent(typeof(EntityReference))]
public class NetworkDamageReceiver : NetworkBehaviour
{
    private PlayerStatsManager _statsManager;
    private GameplayHUD _gameplayHUD;

    private void Awake()
    {
        _statsManager = FindObjectOfType<PlayerStatsManager>();
        _gameplayHUD = FindObjectOfType<GameplayHUD>();
    }

    // Изменили название и логику: теперь это регистрация попадания, а не приказ отнять здоровье
    [ServerRpc(RequireOwnership = false)]
    public void RegisterHitServerRpc(int baseDamage, ServerRpcParams serverRpcParams = default)
    {
        ulong senderClientId = serverRpcParams.Receive.SenderClientId;
        ApplyDamageOnServer(baseDamage, senderClientId);
    }

    public void ApplyDamageOnServer(int baseDamage, ulong attackerClientId)
    {
        // Проверка прав (работает только на сервере)
        if (NetworkManager.Singleton != null && !NetworkManager.Singleton.IsServer) return;
        if (baseDamage <= 0) return;

        // СЕРВЕРНАЯ ПРОВЕРКА ЩИТА (Защита от читеров, отключивших щит у себя на клиенте)
        if (TryGetComponent<CharacterEffectController>(out var shield) && shield.IsShieldActive)
        {
            Debug.Log($"[Сервер] Урон по {gameObject.name} отклонен: сервер подтвердил активность щита!");
            return;
        }

        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        var entityRef = GetComponent<EntityReference>();

        if (entityRef == null || entityRef.myEntity == Entity.Null || !entityManager.HasComponent<HealthData>(entityRef.myEntity)) return;

        var healthData = entityManager.GetComponentData<HealthData>(entityRef.myEntity);
        bool wasAlive = healthData.Value > 0;

        // Сервер САМ применяет CombatRules для надежного расчета
        healthData.Value = CombatRules.ApplyDamage(healthData.Value, baseDamage);

        entityManager.SetComponentData(entityRef.myEntity, healthData);
        SyncMonoHealth(healthData.Value);

        // Рассылаем официальный новый запас здоровья всем клиентам
        TakeDamageClientRpc(healthData.Value, wasAlive && healthData.Value <= 0, attackerClientId);
    }

    [ClientRpc]
    private void TakeDamageClientRpc(int newHealth, bool died, ulong senderClientId)
    {
        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        var entityRef = GetComponent<EntityReference>();

        if (entityRef != null && entityRef.myEntity != Entity.Null)
        {
            if (entityManager.HasComponent<HealthData>(entityRef.myEntity))
            {
                var healthData = entityManager.GetComponentData<HealthData>(entityRef.myEntity);

                // Синхронизируем здоровье с серверным
                healthData.Value = newHealth;
                entityManager.SetComponentData(entityRef.myEntity, healthData);

                SyncMonoHealth(healthData.Value);

                // Если сервер подтвердил смерть — выдаем награду убийце
                if (died && EntityManagerHasEnemyTag(entityManager, entityRef.myEntity))
                {
                    if (senderClientId == NetworkManager.Singleton.LocalClientId)
                    {
                        if (_statsManager != null)
                        {
                            //  Локальная статистика
                            _statsManager.stats.killsCount++;
                            _statsManager.stats.coinsCount++;

                            // Глобальная статистика
                            _statsManager.stats.globalTotalKills++;
                            _statsManager.stats.globalTotalCoinsEarned++;

                            // Статистика по конкретному монстру
                            if (TryGetComponent<EnemyComponent>(out var enemyComp))
                            {
                                int mId = enemyComp.monsterID;

                                // Прибавляем в глобальный массив(для достижений в главном меню)
                                if (_statsManager.stats.monsterKillsByID != null &&
                                    mId >= 0 && mId < _statsManager.stats.monsterKillsByID.Length)
                                {
                                    _statsManager.stats.monsterKillsByID[mId]++;
                                }

                                // Прибавляем в локальный массив (для HUD на текущей сцене)
                                if (_statsManager.stats.matchMonsterKillsByID != null &&
                                    mId >= 0 && mId < _statsManager.stats.matchMonsterKillsByID.Length)
                                {
                                    _statsManager.stats.matchMonsterKillsByID[mId]++;
                                }
                            }

                            if (_gameplayHUD != null) _gameplayHUD.InitializeHUD();

                            Debug.Log("[Network] Сервер подтвердил убийство! Награда зачислена.");
                        }
                    }
                }
            }
        }
    }

    private static bool EntityManagerHasEnemyTag(EntityManager entityManager, Entity entity)
    {
        return entityManager.HasComponent<EnemyTag>(entity);
    }

    private void SyncMonoHealth(int value)
    {
        if (TryGetComponent<PlayerComponent>(out var playerComp))
        {
            playerComp.health = value;
        }
        else if (TryGetComponent<EnemyComponent>(out var enemyComp))
        {
            enemyComp.health = value;
        }
    }
}