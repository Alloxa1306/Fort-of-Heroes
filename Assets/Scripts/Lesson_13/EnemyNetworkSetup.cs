using UnityEngine;
using Unity.Netcode;

public class EnemyNetworkSetup : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        EnemySpawner spawner = FindObjectOfType<EnemySpawner>();
        if (spawner != null)
        {
            // Определяем, сервер ли мы 
            spawner.InitEnemy(gameObject, IsServer);
        }
        else
        {
            Debug.LogError("[EnemyNetworkSetup] Не найден EnemySpawner на сцене!");
        }
    }
}