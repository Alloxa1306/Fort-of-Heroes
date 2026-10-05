using Unity.Entities;
using UnityEngine;
using UnityEngine.AI;

namespace Systems
{
    public class EnemyAnimSystem : ComponentSystem
    {
        private EntityQuery animQuery;

        protected override void OnCreate()
        {
            animQuery = GetEntityQuery(
                ComponentType.ReadOnly<AIAgent>(),
                ComponentType.ReadOnly<Animator>(),
                ComponentType.ReadOnly<NavMeshAgent>(),
                ComponentType.ReadOnly<EnemyAnimationAuth>()
            );
        }

        protected override void OnUpdate()
        {
            Entities.With(animQuery).ForEach((Entity entity, Animator animator, NavMeshAgent agent, EnemyAnimationAuth animAuth) =>
            {
                if (animator == null || agent == null) return;

                // —читываем скорость агента
                float currentSpeed = agent.velocity.magnitude;

                // ѕередаем в аниматор по быстрому хэшу
                animator.SetFloat(animAuth.SpeedHash, currentSpeed);
            });
        }
    }
}