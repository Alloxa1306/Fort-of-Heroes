using UnityEngine;
using UnityEngine.AI;
using DefaultNamespace.Components.Interfaces;

public class WaitBehaviour : MonoBehaviour, IBehaviour
{
    private NavMeshAgent agent;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    public float Evaluate()
    {
        // Базовый приоритет. Если другие поведения вернули 0, враг просто стоит
        return 0.1f;
    }

    public void Behave()
    {
        if (agent != null && agent.enabled)
        {
            agent.isStopped = true; // Останавливаем агента
        }
    }
}