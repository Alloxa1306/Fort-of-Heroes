using UnityEngine;
using DG.Tweening;

public class HeavyTrapAnimation : MonoBehaviour
{
    [Header("Настройки движения")]
    [SerializeField] private float liftHeight = 4f;       // Высота подъема плиты
    [SerializeField] private float riseDuration = 2.5f;   // Время плавного подъема
    [SerializeField] private float fallDuration = 0.25f;  // Время резкого падения

    [Header("Тайминги пауз")]
    [SerializeField] private float topPause = 1.0f;       // Сколько ждет наверху перед ударом
    [SerializeField] private float bottomPause = 1.5f;    // Сколько лежит на земле после удара

    private Vector3 startPosition;
    private bool isFalling;

    // Свойство, чтобы скрипт урона знал, летит ли плита вниз прямо сейчас
    public bool IsFalling => isFalling;

    void Start()
    {
        startPosition = transform.position;
        InitTrapSequence();
    }

    private void InitTrapSequence()
    {
        // Создаем последовательность анимаций
        Sequence trapSequence = DOTween.Sequence();

        // Медленно и плавно поднимаем плиту вверх  , SetEase(Ease.InOutQuad) - плавно сглаживает движения поднятие
        trapSequence.Append(transform.DOMoveY(startPosition.y + liftHeight, riseDuration).SetEase(Ease.InOutQuad));

        // Делаем  паузу на пике, давая игроку шанс пробежать
        trapSequence.AppendInterval(topPause);

        // Включаем флаг падения прямо перед ударом , позволяет выполнить любой скрипт
        trapSequence.AppendCallback(() => isFalling = true);

        // Резко роняем плиту вниз с эффектом ускорения , SetEase(Ease.InQuad) - плавно сглаживает падение
        trapSequence.Append(transform.DOMoveY(startPosition.y, fallDuration).SetEase(Ease.InQuad));

        // Плита ударилась о землю — выключаем опасность и делаем передышку
        trapSequence.AppendCallback(() => isFalling = false);
        trapSequence.AppendInterval(bottomPause);

        // Делаем эту цепочку бесконечной
        trapSequence.SetLoops(-1);
    }
}