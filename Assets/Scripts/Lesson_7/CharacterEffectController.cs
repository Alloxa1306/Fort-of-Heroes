using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterEffectController : MonoBehaviour
{
    private List<Material> shieldMaterials = new List<Material>();
    private Coroutine effectCoroutine;

    private const string ShieldIntensityProperty = "_ShieldIntensity";

    [Header("Настройки эффекта щита")]
    [SerializeField] private float defaultDuration = 3f;  // Длительность по умолчанию
    [SerializeField] private float fadeSpeed = 5f;        // Скорость появления
    [SerializeField] private float maxIntensity = 15f;

    public bool IsShieldActive { get; private set; }

    private void Awake()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        foreach (var ren in renderers)
        {
            Material mat = ren.material;
            if (mat.HasProperty(ShieldIntensityProperty))
            {
                shieldMaterials.Add(mat);
            }
        }

        if (shieldMaterials.Count == 0)
        {
            Debug.LogError($" [{gameObject.name}] Не нашёл ни одного дочернего объекта с материалом {ShieldIntensityProperty}!");
        }
    }

    // Вызываем метод активации
    public void ActivateShieldEffect(float duration = -1f)
    {
        if (shieldMaterials.Count == 0) return;

        float activeDuration = duration > 0f ? duration : defaultDuration;

        if (effectCoroutine != null)
        {
            StopCoroutine(effectCoroutine);
        }

        effectCoroutine = StartCoroutine(ShieldRoutine(activeDuration));
    }

    private IEnumerator ShieldRoutine(float activeDuration)
    {
        IsShieldActive = true; // Включаем неуязвимость

        float currentIntensity = shieldMaterials[0].GetFloat(ShieldIntensityProperty);

        // Плавное проявление щита
        while (currentIntensity < maxIntensity)
        {
            currentIntensity += Time.deltaTime * fadeSpeed;
            SetIntensityForAllMaterials(Mathf.Clamp(currentIntensity, 0f, maxIntensity));
            yield return null;
        }

        // Ожидание работы щита
        yield return new WaitForSeconds(activeDuration);

        // Плавное затухание
        while (currentIntensity > 0f)
        {
            currentIntensity -= Time.deltaTime * fadeSpeed;
            SetIntensityForAllMaterials(Mathf.Clamp(currentIntensity, 0f, maxIntensity));
            yield return null;
        }

        SetIntensityForAllMaterials(0f);
        effectCoroutine = null;
        IsShieldActive = false; // Выключаем неуязвимость
    }

    private void SetIntensityForAllMaterials(float value)
    {
        foreach (var mat in shieldMaterials)
        {
            if (mat != null)
            {
                mat.SetFloat(ShieldIntensityProperty, value);
            }
        }
    }

    private void OnDestroy()
    {
        foreach (var mat in shieldMaterials)
        {
            if (mat != null) Destroy(mat);
        }
    }
}