using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class AudioSettingsManager : MonoBehaviour
{
    [Header("Слайдеры настроек")]
    public Slider musicSlider;
    public Slider sfxSlider;
    public Slider uiSlider;

    private FMOD.Studio.Bus musicBus;
    private FMOD.Studio.Bus sfxBus;
    private FMOD.Studio.Bus uiBus;

    private bool isInitialized = false;

    // Дожидавемся данных
    private IEnumerator Start()
    {
        musicBus = FMODUnity.RuntimeManager.GetBus("bus:/Music");
        sfxBus = FMODUnity.RuntimeManager.GetBus("bus:/SFX");
        uiBus = FMODUnity.RuntimeManager.GetBus("bus:/UI");

        // Скрипт замирает на этой строчке и ждет, пока менеджер загрузит данные
        yield return new WaitUntil(() => GameDataManager.Instance != null && GameDataManager.Instance.isDataLoaded);

        // Настраиваем ползунки, передавая в метод шину и действие для сохранения
        SetupSlider(musicSlider, GameDataManager.Instance.Settings.musicVolume, musicBus, vol => GameDataManager.Instance.Settings.musicVolume = vol);
        SetupSlider(sfxSlider, GameDataManager.Instance.Settings.soundVolume, sfxBus, vol => GameDataManager.Instance.Settings.soundVolume = vol);
        SetupSlider(uiSlider, GameDataManager.Instance.Settings.uiVolume, uiBus, vol => GameDataManager.Instance.Settings.uiVolume = vol);

        isInitialized = true;
    }

    // Универсальный метод для всех слайдеров
    private void SetupSlider(Slider slider, float savedVolume, FMOD.Studio.Bus bus, System.Action<float> saveAction)
    {
        if (slider == null) return;

        slider.onValueChanged.RemoveAllListeners();

        slider.value = savedVolume;
        bus.setVolume(savedVolume);

        // Подписываемся на изменение ползунка игроком
        slider.onValueChanged.AddListener(vol =>
        {
            bus.setVolume(vol);

            if (isInitialized && GameDataManager.Instance != null)
            {
                saveAction(vol); // Обновляем конкретную переменную в Settings
                GameDataManager.Instance.SaveSettingsLocal();
            }
        });
    }

    public void ResetAudioSettings()
    {
        if (!isInitialized) return;

        if (musicSlider != null) musicSlider.value = 1f;
        if (sfxSlider != null) sfxSlider.value = 1f;
        if (uiSlider != null) uiSlider.value = 1f;
    }
}