using System.Collections.Generic;
using UnityEngine;

public class ApplyHeal : MonoBehaviour, IAbilityTarget
{
    [Header("Настройка лечения")]
    public int healAmount = 20;

    public List<GameObject> Targets { get; set; } = new List<GameObject>();

    public void Execute()
    {
        foreach (var target in Targets)
        {
            if (target == this.gameObject) continue;

            // Просто ищем PlayerComponent на цели
            if (target.TryGetComponent<PlayerComponent>(out var playerComponent))
            {
                // Передаем лечение компоненту. Он сам применит CombatRules и обновит ECS!
                playerComponent.ApplyHeal(healAmount);

                Debug.Log($"Аптечка [{gameObject.name}] вылечила {target.name} на {healAmount} HP!");
            }
        }
    }
}