using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [Header("Ссылки на компоненты Игрока")]
    [SerializeField] private PlayerMoveComponent moveComponent;
    [SerializeField] private PlayerWeaponComponent weaponComponent;
    [SerializeField] private PlayerComponent playerComponent;
    [SerializeField] private EntityReference entityReference;
    [SerializeField] private CharacterEffectController effectController;

    [Header("Максимум слотов в инвентаре")]
    [SerializeField] private int maxSlots = 6;

    private readonly List<ItemConfig> items = new List<ItemConfig>();

    public event Action OnInventoryChanged;
    public IReadOnlyList<ItemConfig> Items => items;

    private void Awake()
    {
        if (moveComponent == null) moveComponent = GetComponent<PlayerMoveComponent>();
        if (weaponComponent == null) weaponComponent = GetComponent<PlayerWeaponComponent>();
        if (playerComponent == null) playerComponent = GetComponent<PlayerComponent>();
        if (entityReference == null) entityReference = GetComponent<EntityReference>();
        if (effectController == null) effectController = GetComponent<CharacterEffectController>();
    }

    public bool AddItem(ItemConfig item)
    {
        if (items.Count >= maxSlots)
        {
            Debug.LogWarning("[Inventory] Инвентарь полон!");
            return false;
        }

        items.Add(item);
        Debug.Log($"[Inventory] Подобран предмет: {item.itemName}");
        OnInventoryChanged?.Invoke();
        return true;
    }

    public void UseItem(ItemConfig item)
    {
        if (!items.Contains(item)) return;

        ApplyPerkEffect(item);

        items.Remove(item);
        OnInventoryChanged?.Invoke();
    }

    private void ApplyPerkEffect(ItemConfig item)
    {
        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        // Перепроверяем ссылку, если она была null при Awake
        if (entityReference == null)
            entityReference = GetComponent<EntityReference>();

        bool hasECS = entityReference != null && entityReference.myEntity != Entity.Null;

        switch (item.perkType)
        {
               //Перк лечения
            case PerkType.Heal:
                if(playerComponent != null)
                {
                    playerComponent.ApplyHeal((int)item.value);
                    Debug.Log($"[Perk] Применено зелье здоровья: +{item.value}");
                }
                break;
                //Перк ускорения персонажа
            case PerkType.SpeedBoost:
                StartCoroutine(SpeedBoostRoutine(item.value, item.duration));
                break;
                //Перк ускорения стрельбы
            case PerkType.FireRateBoost:
                StartCoroutine(FireRateBoostRoutine(item.value, item.duration));
                break;
            // Перк рикошета
            case PerkType.BounceAmmo:
                if (hasECS)
                {
                    // Всегда перезаписываем компонент с новым временем!
                    BouncePerkData newPerkData = new BouncePerkData { remainingTime = item.duration };

                    if (entityManager.HasComponent<BouncePerkData>(entityReference.myEntity))
                    {
                        entityManager.SetComponentData(entityReference.myEntity, newPerkData);
                    }
                    else
                    {
                        entityManager.AddComponentData(entityReference.myEntity, newPerkData);
                    }

                    Debug.Log($"[Perk] Активированы рикошетные патроны на {item.duration} сек!");
                }
                break;
            //Перк щита
            case PerkType.Shield:
                if (effectController != null)
                {
                    effectController.ActivateShieldEffect(item.duration);
                    Debug.Log($"[Perk] Щит активирован на {item.duration} секунд!");
                }
                break;
        }
    }

    // Корутина временного ускорения бега
    private IEnumerator SpeedBoostRoutine(float boostValue, float duration)
    {
        if (moveComponent == null) yield break;

        // Применяем ускорение
        moveComponent.moveSpeed += boostValue;
        if (playerComponent != null) playerComponent.moveSpeed = moveComponent.moveSpeed;
        Debug.Log($"[Perk] Скорость увеличена на +{boostValue} на {duration} секунд!");

        // Ждем указанное время
        yield return new WaitForSeconds(duration);

        // Возвращаем исходную скорость
        moveComponent.moveSpeed -= boostValue;
        if (playerComponent != null) playerComponent.moveSpeed = moveComponent.moveSpeed;
        Debug.Log("[Perk] Действие ускорения завершилось.");
    }

    // Корутина временного увеличения скорострельности
    private IEnumerator FireRateBoostRoutine(float boostValue, float duration)
    {
        if (weaponComponent == null) yield break;

        // Уменьшаем паузу между выстрелами
        weaponComponent.fireRate = Mathf.Max(0.05f, weaponComponent.fireRate - boostValue);
        if (playerComponent != null) playerComponent.fireRate = weaponComponent.fireRate;
        Debug.Log($"[Perk] Быстрая стрельба активирована на {duration} секунд!");

        // Ждем указанное время
        yield return new WaitForSeconds(duration);

        // Возвращаем исходную паузу
        weaponComponent.fireRate += boostValue;
        if (playerComponent != null) playerComponent.fireRate = weaponComponent.fireRate;
        Debug.Log("[Perk] Действие быстрой стрельбы завершилось.");
    }
}