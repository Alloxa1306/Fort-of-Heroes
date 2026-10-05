using UnityEngine;
using UnityEngine.AI;
using DefaultNamespace.Components.Interfaces;

public class PatrolBehaviour : MonoBehaviour, IBehaviour
{
    private NavMeshAgent agent;
    private EnemyComponent enemyComponent;

    private Vector3 currentDestination;
    private bool hasDestination = false;

    private float waitTimer = 0f;
    public float waitTimeAtPoint = 2f;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        enemyComponent = GetComponent<EnemyComponent>();
    }

    public float Evaluate()
    {
        // ѕриоритет 0.5 Ч выше, чем у Wait (0.1), но ниже, чем у Chase (1.0)
        return 0.5f;
    }

    public void Behave()
    {
        // ƒобавлена проверка !agent.isOnNavMesh, чтобы избежать ошибок при спавне
        if (agent == null || !agent.enabled || !agent.isOnNavMesh) return;

        agent.isStopped = false;

        // ≈сли моб дошел до точки или у него ее еще нет
        if (!hasDestination || agent.remainingDistance <= agent.stoppingDistance)
        {
            waitTimer += Time.deltaTime;

            // —тоим на месте waitTimeAtPoint секунд, затем ищем новую точку
            if (waitTimer >= waitTimeAtPoint)
            {
                FindRandomPoint();
                waitTimer = 0f;
            }
        }
    }

    private void FindRandomPoint()
    {
        // «абираем данные из EnemyComponent 
        Vector3 center = enemyComponent.PatrolCenter;
        float radius = enemyComponent.PatrolRadius;

        Vector2 randomCircle = Random.insideUnitCircle * radius;
        Vector3 randomPos = new Vector3(center.x + randomCircle.x, center.y, center.z + randomCircle.y);

        if (NavMesh.SamplePosition(randomPos, out NavMeshHit hit, radius, NavMesh.AllAreas))
        {
            currentDestination = hit.position;
            agent.SetDestination(currentDestination);
            hasDestination = true;
        }
        else
        {
            hasDestination = false;
        }
    }
}