using Unity.Entities;

namespace Systems
{
    public class AIBehaveSystem : ComponentSystem
    {
        private EntityQuery behaveQuery;

        protected override void OnCreate()
        {
            behaveQuery = GetEntityQuery(ComponentType.ReadOnly<AIAgent>());
        }

        protected override void OnUpdate()
        {
            Entities.With(behaveQuery).ForEach((Entity entity, BehaviourManager manager) =>
            {
                if (manager != null && manager.activeBehaviour != null)
                {
                    manager.activeBehaviour?.Behave();
                }
            });
        }
    }
}
