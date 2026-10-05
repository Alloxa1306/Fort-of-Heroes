using UnityEngine;
using UnityEngine.AI;
using Unity.Netcode;
using DefaultNamespace.Components.Interfaces;

public class AttackBehaviour : MonoBehaviour, IBehaviour
{
    private NavMeshAgent agent;
    private Transform targetPlayer;
    private Animator animator;
    private EnemyAnimationAuth animAuth;
    private EnemyComponent enemyComponent;

    public float attackCooldown = 1.5f;
    [SerializeField] private float hitRadius = 1.4f;
    private float lastAttackTime;

    void Awake()
    {
        enemyComponent = GetComponent<EnemyComponent>();
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        animAuth = GetComponentInChildren<EnemyAnimationAuth>();
    }

    public float Evaluate()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        if (players.Length == 0) return 0f;

        float closestDistance = Mathf.Infinity;
        targetPlayer = null;

        foreach (var p in players)
        {
            if (p == null) continue;

            float dist = Vector3.Distance(transform.position, p.transform.position);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                targetPlayer = p.transform;
            }
        }

        if (targetPlayer == null) return 0f;

        float currentAttackRange = enemyComponent != null ? enemyComponent.attackRange : 2f;

        if (closestDistance <= currentAttackRange)
        {
            return 2f;
        }

        return 0f;
    }

    public void Behave()
    {
        if (agent != null && agent.enabled)
        {
            agent.isStopped = true;
        }

        if (Time.time >= lastAttackTime + attackCooldown)
        {
            if (animator != null && animAuth != null)
            {
                animator.SetTrigger(animAuth.AttackHash);
            }
            lastAttackTime = Time.time;
        }
    }

    public void ExecuteAttackLogic()
    {
        DealDamageToTarget();
    }

    private void DealDamageToTarget()
    {
        if (NetworkManager.Singleton != null && !NetworkManager.Singleton.IsServer) return;
        if (targetPlayer == null || enemyComponent == null) return;

        // --- НОВАЯ МЕХАНИКА УКЛОНЕНИЯ ---
        // Берем базовую дальность атаки монстра и добавляем небольшой запас (0.5f), 
        // чтобы атаки не мазали, если игрок отошел буквально на миллиметр
        float currentHitRange = enemyComponent.attackRange + 0.5f;
        float distanceToTarget = Vector3.Distance(transform.position, targetPlayer.position);

        // Если в кадре удара игрок находится дальше радиуса поражения — отменяем урон
        if (distanceToTarget > currentHitRange)
        {
            Debug.Log($"[Монстр] Промах! Игрок увернулся. Дистанция: {distanceToTarget:F1} > {currentHitRange}");
            return;
        }
        // --------------------------------

        int damageValue = Mathf.RoundToInt(enemyComponent.damage);
        if (damageValue <= 0) return;
        Debug.Log($"Удар! Урон: {damageValue} | Кадр: {Time.frameCount}");
        NetworkDamageReceiver targetReceiver = targetPlayer.GetComponent<NetworkDamageReceiver>() ??
                                               targetPlayer.GetComponentInParent<NetworkDamageReceiver>() ??
                                               targetPlayer.GetComponentInChildren<NetworkDamageReceiver>();
        if (targetReceiver != null && targetReceiver.CompareTag("Player"))
        {
            targetReceiver.ApplyDamageOnServer(damageValue, 0UL);
            return;
        }

        Collider[] hits = Physics.OverlapSphere(targetPlayer.position, hitRadius, ~0, QueryTriggerInteraction.Collide);
        foreach (Collider hit in hits)
        {
            if (!hit.CompareTag("Player") && !hit.transform.root.CompareTag("Player"))
            {
                continue;
            }

            NetworkDamageReceiver receiver = hit.GetComponentInParent<NetworkDamageReceiver>() ??
                                             hit.GetComponentInChildren<NetworkDamageReceiver>();
            if (receiver != null && receiver.CompareTag("Player"))
            {
                receiver.ApplyDamageOnServer(damageValue, 0UL);
                break;
            }
        }
    }
}
