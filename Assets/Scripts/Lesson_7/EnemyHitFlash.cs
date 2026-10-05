using System.Collections;
using UnityEngine;

public class EnemyHitFlash : MonoBehaviour
{
    [Header("Настройки вспышки")]
    [SerializeField] private Color flashColor = Color.white;
    [SerializeField] private float flashDuration = 0.1f;

    [Header("Рендереры (настроятся сами)")]
    [SerializeField] private Renderer[] renderers;

    private Coroutine _flashCoroutine;

    private void Awake()
    {
        if (renderers == null || renderers.Length == 0)
        {
            renderers = GetComponentsInChildren<Renderer>();
        }
    }

    public void PlayFlash()
    {
        if (_flashCoroutine != null)
        {
            StopCoroutine(_flashCoroutine);
        }
        _flashCoroutine = StartCoroutine(FlashCoroutine());
    }

    private IEnumerator FlashCoroutine()
    {
        // Проверяем, что корутина запустилась и красит врага
        SetShaderProperties(1f, flashColor);

        yield return new WaitForSeconds(flashDuration);

        // Проверяем, что время вышло и цвет возвращается назад
        SetShaderProperties(0f, Color.black);

        _flashCoroutine = null;
    }

    private void SetShaderProperties(float blend, Color color)
    {
        foreach (var renderer in renderers)
        {
            if (renderer == null) continue;

            // Изменяем свойства напрямую у материала.
            renderer.material.SetFloat("_HitBlend", blend);
            renderer.material.SetColor("_HitColor", color);
        }
    }

    private void OnDestroy()
    {
        // Уничтожаем созданные копии материалов при удалении врага
        if (renderers != null)
        {
            foreach (var renderer in renderers)
            {
                if (renderer != null && renderer.material != null)
                {
                    Destroy(renderer.material);
                }
            }
        }
    }
}