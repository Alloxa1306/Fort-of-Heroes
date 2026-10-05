using System.Collections.Generic;
using UnityEngine;

public class BulletPool : MonoBehaviour
{
    public static BulletPool Instance { get; private set; }

    [Header("Настройки пула")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private int poolSize = 30; // Запас пуль при старте

    private Queue<GameObject> pool = new Queue<GameObject>();

    private void Awake()
    {
        Instance = this;
        InitializePool();
    }

    private void InitializePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(bulletPrefab, transform);
            obj.SetActive(false); // Выключаем пулю
            pool.Enqueue(obj);
        }
    }

    public GameObject GetBullet(Vector3 position, Quaternion rotation)
    {
        if (pool.Count > 0)
        {
            GameObject obj = pool.Dequeue();
            obj.transform.position = position;
            obj.transform.rotation = rotation;
            obj.SetActive(true);
            return obj;
        }
        else
        {
            // Если мы стреляем как из пулемета и 30 пуль не хватило — создаем запасную
            GameObject obj = Instantiate(bulletPrefab, position, rotation, transform);
            return obj;
        }
    }

    public void ReturnBullet(GameObject obj)
    {
        obj.SetActive(false); // Просто гасим объект вместо Destroy
        pool.Enqueue(obj);
    }
}