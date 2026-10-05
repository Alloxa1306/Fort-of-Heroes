using UnityEngine;
using UnityEngine.AI;
using DefaultNamespace.Components.Interfaces;

public class ChaseBehaviour : MonoBehaviour, IBehaviour
{
    private NavMeshAgent agent;
    private Transform targetPlayer;
    private EnemyComponent enemyComponent;
    public float visionDistance = 15f;

    void Awake()
    {
        enemyComponent = GetComponent<EnemyComponent>();
        agent = GetComponent<NavMeshAgent>();
        // Убрали поиск игрока из Awake!
    }

    public float Evaluate()
    {
        // Находим всех игроков на сцене
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        if (players.Length == 0) return 0f;

        float closestDistance = Mathf.Infinity;
        targetPlayer = null;

        // Ищем ближайшего
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

        // Если ближайший игрок в зоне видимости, но далеко для удара — бежим за ним
        if (closestDistance > currentAttackRange && closestDistance < visionDistance)
        {
            return 1f; // Высокий приоритет для погони
        }

        return 0f;
    }

    public void Behave()
    {
        if (targetPlayer != null && agent != null && agent.enabled)
        {
            agent.isStopped = false;
            agent.SetDestination(targetPlayer.position);
        }
    }
}