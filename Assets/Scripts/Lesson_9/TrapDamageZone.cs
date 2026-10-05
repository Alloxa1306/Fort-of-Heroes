using UnityEngine;

[RequireComponent(typeof(ApplyDamage))]
public class TrapDamageZone : MonoBehaviour
{
    [SerializeField] private HeavyTrapAnimation trapAnimation;
    private ApplyDamage damageApplier;

    void Awake()
    {
        damageApplier = GetComponent<ApplyDamage>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // Ловушка реагирует только на Игрока
        if (other.CompareTag("Player"))
        {
            // Наносим урон только в фазе падения плиты вниз
            if (trapAnimation != null && trapAnimation.IsFalling)
            {
                // Очищаем старые цели, добавляем игрока и бьем
                damageApplier.Targets.Clear();
                damageApplier.Targets.Add(other.gameObject);
                damageApplier.Execute();
            }
        }
    }
}