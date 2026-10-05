using UnityEngine;
using UnityEngine.UI;
using System;

[RequireComponent(typeof(Button), typeof(CanvasGroup))]
public class QuickSlotButton : MonoBehaviour
{
    [Header("За какой предмет отвечает эта кнопка?")]
    public PerkType slotPerkType;

    private Button buttonComponent;
    private CanvasGroup canvasGroup;
    private Action onConsume;

    private void Awake()
    {
        buttonComponent = GetComponent<Button>();
        canvasGroup = GetComponent<CanvasGroup>();

        buttonComponent.onClick.AddListener(UseSlot);
        SetEmpty(); // При старте кнопка полупрозрачная
    }

    public void SetActive(Action consumeAction)
    {
        canvasGroup.alpha = 1f; // Делаем кнопку яркой
        buttonComponent.interactable = true;
        onConsume = consumeAction;
    }

    public void SetEmpty()
    {
        canvasGroup.alpha = 0.4f; // Полупрозрачная в пустом состоянии
        buttonComponent.interactable = false;
        onConsume = null;
    }

    private void UseSlot()
    {
        onConsume?.Invoke();
        SetEmpty(); // Сбрасываем кнопку после применения эффекта
    }

    public bool IsReady()
    {
        return buttonComponent.interactable;
    }
}