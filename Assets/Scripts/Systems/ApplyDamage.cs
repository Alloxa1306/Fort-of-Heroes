using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Netcode;

public class ApplyDamage : MonoBehaviour, IAbilityTarget
{
    public enum DamageSource { Player, Enemy, Trap }

    [Header("Основные настройки")]
    public DamageSource source = DamageSource.Enemy;
    public int damage = 10;
    public float damageCooldown = 1f;

    [HideInInspector] public bool isLocalBullet = false;

    public List<GameObject> Targets { get; set; } = new List<GameObject>();
    private Dictionary<GameObject, float> lastDamageTime = new Dictionary<GameObject, float>();
    private PlayerStatsManager _playerStats;

    private void Awake()
    {
        _playerStats = Object.FindObjectOfType<PlayerStatsManager>();
    }

    public void Execute()
    {
        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        foreach (var target in Targets)
        {
            if (target == this.gameObject || target == null) continue;

            // Клиентское предсказание: визуально блокируем урон, если видим активный щит
            if (target.TryGetComponent<CharacterEffectController>(out var shield) && shield.IsShieldActive)
            {
                Debug.Log($"🛡️ [Клиент] {target.name} заблокировал урон щитом!");
                continue; // Не спамим сервер лишними запросами
            }

            var entityRef = target.GetComponent<EntityReference>();

            if (entityRef != null && entityRef.myEntity != Entity.Null)
            {
                if (!lastDamageTime.ContainsKey(target) || Time.time >= lastDamageTime[target] + damageCooldown)
                {
                    // Мгновенный визуальный отклик на клиенте (без задержек сети)
                    EnemyHitFlash hitFlash = target.GetComponent<EnemyHitFlash>() ??
                                             target.GetComponentInParent<EnemyHitFlash>() ??
                                             target.GetComponentInChildren<EnemyHitFlash>();
                    if (hitFlash != null)
                    {
                        hitFlash.PlayFlash();
                    }

                    // Локальная запись статистики попаданий (чтобы UI обновлялся сразу)
                    if (source == DamageSource.Player && isLocalBullet)
                    {
                        if (_playerStats != null)
                        {
                            _playerStats.stats.hitsCount += 1;
                            _playerStats.stats.totalDamageDealt += damage;
                        }
                    }

                    // Отправка запроса на сервер
                    var networkReceiver = target.GetComponent<NetworkDamageReceiver>();

                    if (networkReceiver != null)
                    {
                        if (isLocalBullet)
                        {
                            // Клиент сообщает серверу
                            networkReceiver.RegisterHitServerRpc(damage);
                        }
                    }
                    else
                    {
                        if (isLocalBullet)
                        {
                            ApplyLocalDamage(target, entityManager, entityRef);
                        }
                    }

                    lastDamageTime[target] = Time.time;
                }
            }
        }
    }

    private void ApplyLocalDamage(GameObject target, EntityManager entityManager, EntityReference entityRef)
    {
        var healthData = entityManager.GetComponentData<HealthData>(entityRef.myEntity);
        healthData.Value -= damage;
        entityManager.SetComponentData(entityRef.myEntity, healthData);

        if (target.TryGetComponent<EnemyComponent>(out var enemyComp))
        {
            enemyComp.health = healthData.Value;
        }
    }
}