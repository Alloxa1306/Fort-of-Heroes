using Zenject;
using UnityEngine;

public class GameInstaller : MonoInstaller
{
    [Header("Переключатель конфигурации")]
    [SerializeField] private bool useDummyConfig;
    [SerializeField] private bool useDummyEnemyConfig;


    [Header("Префабы для спавна")]
    [SerializeField] private GameObject enemyPrefab;

    public override void InstallBindings()
    {
        // Находим наш менеджер данных
        GameDataManager dataManager = FindObjectOfType<GameDataManager>();

        // Конфиг Игрока
        if (useDummyConfig)
        {
            Container.Bind<IPlayerConfig>().To<DummyPlayerConfigProvider>().AsSingle();
        }
        else
        {
            if (dataManager != null && dataManager.PlayerConfig != null)
            {
                Container.Bind<IPlayerConfig>().FromInstance(dataManager.PlayerConfig).AsSingle();
            }
            else
            {
                Debug.LogWarning("[Zenject] GameDataManager не найден! Включаем заглушку игрока.");
                Container.Bind<IPlayerConfig>().To<DummyPlayerConfigProvider>().AsSingle();
            }
        }

        // Конфиг Врага 
        if (useDummyEnemyConfig)
        {
            Container.Bind<IEnemyConfig>().To<DummyEnemyConfigProvider>().AsSingle();
        }
        else
        {
            if (dataManager != null && dataManager.EnemyConfig != null)
            {
                Container.Bind<IEnemyConfig>().FromInstance(dataManager.EnemyConfig).AsSingle();
            }
            else
            {
                Debug.LogWarning("[Zenject] GameDataManager не найден! Включаем заглушку врагов.");
                Container.Bind<IEnemyConfig>().To<DummyEnemyConfigProvider>().AsSingle();
            }
        }

        // Регистрируем фабрику врагов
        Container.BindFactory<EnemyComponent, EnemyComponent.Factory>()
            .FromComponentInNewPrefab(enemyPrefab)
            .UnderTransformGroup("Spawned_Enemies");

        Container.Bind<EnemySpawner>().FromComponentInHierarchy().AsSingle().NonLazy();
    }
}