using UnityEngine;
using Zenject;

public class PlayerComponent : MonoBehaviour
{
    public int health;
    public int maxHealth;
    public float moveSpeed;
    public float fireRate;
    public float dashDistance;
    public float turnSpeed;

    [Inject]
    public void Construct(IPlayerConfig config)
    {
        if (config != null && config.HeroHealth > 0)
        {
            health = config.HeroHealth;
            maxHealth = config.HeroHealth;
            moveSpeed = config.MoveSpeed;
            fireRate = config.FireRate;
            dashDistance = config.DashDistance;
            turnSpeed = config.TurnSpeed;
        }

        else
        {
            maxHealth = 100;
            health = 100;
            moveSpeed = 5f;
            fireRate = 0.1f;
            dashDistance = 3f;
            turnSpeed = 15f;
        }
    }

    public void ApplyHeal(int healAmount)
    {
        health = CombatRules.ApplyHealing(health, maxHealth, healAmount);

        var entityManager = Unity.Entities.World.DefaultGameObjectInjectionWorld.EntityManager;
        var entityRef = GetComponent<EntityReference>();

        if (entityRef != null && entityRef.myEntity != Unity.Entities.Entity.Null)
        {
            if (entityManager.HasComponent<HealthData>(entityRef.myEntity))
            {
                var healthData = entityManager.GetComponentData<HealthData>(entityRef.myEntity);
                healthData.Value = health;
                entityManager.SetComponentData(entityRef.myEntity, healthData);
            }
        }
    }
}