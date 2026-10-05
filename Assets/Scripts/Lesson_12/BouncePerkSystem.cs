using Unity.Entities;
using UnityEngine;

public class BouncePerkSystem : ComponentSystem
{
    private EntityQuery query;

    protected override void OnCreate()
    {
        query = GetEntityQuery(ComponentType.ReadWrite<BouncePerkData>());
    }

    protected override void OnUpdate()
    {
        float deltaTime = Time.DeltaTime;

        Entities.With(query).ForEach((Entity entity, ref BouncePerkData bounceData) =>
        {
            bounceData.remainingTime -= deltaTime;

            if (bounceData.remainingTime <= 0f)
            {
                PostUpdateCommands.RemoveComponent<BouncePerkData>(entity);
                Debug.Log("[Perk] Действие рикошета завершилось!");
            }
        });
    }
}