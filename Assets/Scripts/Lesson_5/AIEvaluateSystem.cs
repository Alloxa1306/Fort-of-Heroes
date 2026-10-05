using Unity.Entities;
using UnityEngine;
using DefaultNamespace.Components.Interfaces;

namespace Systems
{
    public class AIEvaluateSystem : ComponentSystem
    {
        private EntityQuery evaluateQuery;

        private float timer;

        private const float EVALUATE_INTERVAL = 0.2f;

        protected override void OnCreate()
        {
            evaluateQuery = GetEntityQuery(ComponentType.ReadOnly<AIAgent>());
        }

        protected override void OnDestroy()
        {
            Entities.With(evaluateQuery).ForEach((Entity entity, BehaviourManager manager) =>
            {
                manager.cachedBehaviours = null;
                manager.activeBehaviour = null;
            });
        }

        protected override void OnUpdate()
        {
            timer += Time.DeltaTime;

            if (timer < EVALUATE_INTERVAL) return;

            timer = 0f;

            Entities.With(evaluateQuery).ForEach((Entity entity, BehaviourManager manager) =>
            {
                if (manager == null || manager.cachedBehaviours == null) return;

                IBehaviour bestBehaviour = null;

                float highScore = 0f;

                foreach (IBehaviour ai in manager.cachedBehaviours)
                {
                    if (ai == null || (ai as MonoBehaviour) == null) continue;

                    float currentScore = ai.Evaluate();

                    if (currentScore > highScore)
                    {
                        highScore = currentScore;

                        bestBehaviour = ai;
                    }
                }

                manager.activeBehaviour = bestBehaviour;
            });
        }
    }
}
