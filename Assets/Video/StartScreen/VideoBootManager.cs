using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class VideoBootManager : MonoBehaviour
{
    [Header("Настройки")]
    public VideoPlayer videoPlayer;
    public string nextSceneName = "StartScene";

    private bool isVideoFinished = false;


    private void Start()
    {
        // Подписываемся на встроенное событие FMOD/Unity: срабатывает, когда клип дошел до конца
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached += OnVideoFinished;
        }

        // Запускаем фоновую загрузку
        StartCoroutine(LoadGameDataAndScene());
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        isVideoFinished = true;
    }

    private IEnumerator LoadGameDataAndScene()
    {
        // Запускаем загрузку следующей сцены в фоновом режиме
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(nextSceneName);

        // Запрещаем Unity переключать сцену, даже если она уже загрузилась на 100%
        asyncLoad.allowSceneActivation = false;

        // Жде пока GameDataManager скачает все сохранения с Google Drive
        yield return new WaitUntil(() => GameDataManager.Instance != null && GameDataManager.Instance.isDataLoaded);

        //  Ждем, пока доиграет красивая анимация видео
        yield return new WaitUntil(() => isVideoFinished);

        // Как только оба условия выполнены — даем команду на мгновенное переключение сцены
        asyncLoad.allowSceneActivation = true;
    }
}