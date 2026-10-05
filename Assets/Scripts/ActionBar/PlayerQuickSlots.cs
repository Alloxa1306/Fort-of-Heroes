using System;
using System.Collections;
using Unity.Entities;
using UnityEngine;

public class PlayerQuickSlots : MonoBehaviour
{
    [HideInInspector] public QuickSlotButton[] skillButtons;

    [Header("Ссылки на компоненты Игрока")]
    [SerializeField] private PlayerMoveComponent moveComponent;
    [SerializeField] private PlayerWeaponComponent weaponComponent;
    [SerializeField] private PlayerComponent playerComponent;
    [SerializeField] private EntityReference entityReference;
    [SerializeField] private CharacterEffectController effectController;

    private void Awake()
    {
        if (moveComponent == null) moveComponent = GetComponent<PlayerMoveComponent>();
        if (weaponComponent == null) weaponComponent = GetComponent<PlayerWeaponComponent>();
        if (playerComponent == null) playerComponent = GetComponent<PlayerComponent>();
        if (entityReference == null) entityReference = GetComponent<EntityReference>();
        if (effectController == null) effectController = GetComponent<CharacterEffectController>();
    }

    public bool TryPickupItem(ItemConfig item)
    {
        if (skillButtons == null || skillButtons.Length == 0) return false;

        // Ищем на экране кнопку, которая отвечает конкретно за этот тип предмета
        foreach (var btn in skillButtons)
        {
            if (btn.slotPerkType == item.perkType)
            {
                if (!btn.IsReady())
                {
                    // Кнопка найдена и она пустая -> Заряжаем её!
                    Action effectAction = () => ApplyPerkEffect(item);
                    btn.SetActive(effectAction);
                    return true;
                }
                else
                {
                    // Кнопка уже яркая
                    Debug.Log($"[ActionBar] Навык {item.perkType} уже заряжен!");
                    return false; // Предмет останется лежать на земле
                }
            }
        }

        Debug.LogWarning($"[ActionBar] На экране нет кнопки для предмета {item.perkType}!");
        return false;
    }

    private void ApplyPerkEffect(ItemConfig item)
    {
        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        if (entityReference == null) entityReference = GetComponent<EntityReference>();
        bool hasECS = entityReference != null && entityReference.myEntity != Entity.Null;

        switch (item.perkType)
        {
            case PerkType.Heal:
                if (playerComponent != null) playerComponent.ApplyHeal((int)item.value);
                break;
            case PerkType.SpeedBoost:
                StartCoroutine(SpeedBoostRoutine(item.value, item.duration));
                break;
            case PerkType.FireRateBoost:
                StartCoroutine(FireRateBoostRoutine(item.value, item.duration));
                break;
            case PerkType.BounceAmmo:
                if (hasECS)
                {
                    BouncePerkData newPerkData = new BouncePerkData { remainingTime = item.duration };
                    if (entityManager.HasComponent<BouncePerkData>(entityReference.myEntity))
                        entityManager.SetComponentData(entityReference.myEntity, newPerkData);
                    else
                        entityManager.AddComponentData(entityReference.myEntity, newPerkData);
                }
                break;
            case PerkType.Shield:
                if (effectController != null) effectController.ActivateShieldEffect(item.duration);
                break;
        }
    }

    private IEnumerator SpeedBoostRoutine(float boostValue, float duration)
    {
        if (moveComponent == null) yield break;
        moveComponent.moveSpeed += boostValue;
        if (playerComponent != null) playerComponent.moveSpeed = moveComponent.moveSpeed;
        yield return new WaitForSeconds(duration);
        moveComponent.moveSpeed -= boostValue;
        if (playerComponent != null) playerComponent.moveSpeed = moveComponent.moveSpeed;
    }

    private IEnumerator FireRateBoostRoutine(float boostValue, float duration)
    {
        if (weaponComponent == null) yield break;
        weaponComponent.fireRate = Mathf.Max(0.05f, weaponComponent.fireRate - boostValue);
        if (playerComponent != null) playerComponent.fireRate = weaponComponent.fireRate;
        yield return new WaitForSeconds(duration);
        weaponComponent.fireRate += boostValue;
        if (playerComponent != null) playerComponent.fireRate = weaponComponent.fireRate;
    }
}