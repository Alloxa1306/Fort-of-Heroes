using Unity.Entities;

public class BouncePerkTimerSystem : ComponentSystem
{

    private EntityQuery querty;

    protected override void OnCreate()
    {
        querty = GetEntityQuery(ComponentType.ReadWrite<BouncePerkData>());
    }

    protected override void OnUpdate()
    {
        float deltaTime = Time.DeltaTime;

        Entities.With(querty).ForEach((Entity entity, ref BouncePerkData perkData) =>

        {

            if(perkData.remainingTime > 0f)
            {
                perkData.remainingTime -= deltaTime;
                
                if(perkData.remainingTime < 0f)

                {
                    perkData.remainingTime = 0f;
                    UnityEngine.Debug.Log(" Время действия перка «Отскакивание снарядов» закончилось!");
                }
            }
        });
    }
}
